using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a group of player characters acting as a single controllable unit.
/// Have Square-Like/Horizontal/Vertical formation modes, computes row/column indices and counts,
/// exposes formation dimensions and per-column/per-row character counts for downstream systems.
/// </summary>
public class CharacterGroupManager : MonoBehaviour
{
    [Header("Character Settings")]
    public CharacterType activeCharacterType;
    public List<Character> characters = new List<Character>(); // All active characters in group
    public Transform pivot; // Invisible pivot used for input-based movement
    public float formationSpacing = 3f; // Distance between characters

    // Limits
    public int MaxRows = 15;
    public int MaxCols = 14;

    // Computed formation geometry
    public int columns { get; private set; } = 1;
    public int rows { get; private set; } = 1;

    public int idealRows { get; private set; } = 1;
    public int idealCols { get; private set; } = 1;
    public float halfWidth { get; private set; } = 0.5f;
    public float halfLength { get; private set; } = 0.5f;

    public List<float> rowDepth;

    // per-column / per-row counts (updated on reformation)
    public int[] charactersPerColumn { get; private set; } = new int[0];
    public int[] charactersPerRow { get; private set; } = new int[0];

    // per-character grid indices: column and row for each character index
    // length equals characters.Count (keeps index mapping stable across frames)
    public int[] characterColumnIndex { get; private set; } = new int[0];
    public int[] characterRowIndex { get; private set; } = new int[0];

    // cached formation target positions (world-space offsets from pivot)
    private Vector3[] formationPositions;

    // events
    private float lastHalfWidth;
    private float lastHalfLength;
    public event Action<float, float> OnFormationSizeChanged; // (halfWidth, halfLength)
    public event Action<Character> OnCharacterAdded;
    public event Action<Character> OnCharacterRemoved;

    // formation mode
    [SerializeField] private FormationShape selectedShape = FormationShape.Square;
    public enum FormationShape { Square, Horizontal, Vertical }

    void Start()
    {
        CacheAllFormationData();
        UpdateFormationShape();
        StartCoroutine(FormationUpdateLoop(0.01f));
    }

    IEnumerator FormationUpdateLoop(float interval)
    {
        while (true)
        {
            ApplyFormation();
            yield return new WaitForSeconds(interval);
        }
    }

    #region Public API
    public void SetFormationShape(FormationShape shape)
    {
        if (selectedShape == shape) return;
        selectedShape = shape;
        UpdateFormationShape();
    }

    public FormationShape GetFormationShape() => selectedShape;

    public void AddCharacter(Character character)
    {
        if (character == null || characters.Contains(character)) return;
        characters.Add(character);
        OnCharacterAdded?.Invoke(character);
        CacheAllFormationData();
    }

    public void RemoveCharacter(Character character)
    {
        if (character == null || !characters.Contains(character)) return;
        characters.Remove(character);
        OnCharacterRemoved?.Invoke(character);
        CacheAllFormationData();
    }

    /// <summary>
    /// Forces recompute (useful if external changes to formationSpacing or pivot happen)
    /// </summary>
    public void RebuildFormation()
    {
        CacheAllFormationData();
    }
    #endregion


    [System.Serializable]
    public struct PlayerFormationData
    {
        public int maxRows;
        public int maxColumns;
        public int rows;
        public int columns;
        public float halfWidth;
        public float halfLength;
        public int[] charactersPerRow;
        public int[] charactersPerColumn;
        public Vector3[] formationPositions;
        public List<float> rowDepth; // distance of each row from pivot
    }

    public Dictionary<FormationShape, PlayerFormationData> cachedFormations = new Dictionary<FormationShape, PlayerFormationData>();


    /// <summary>
    /// Calculates target positions for each character in a grid formation and updates helper arrays.
    /// Positions are offsets relative to pivot; caller uses pivot.position + formationPositions[i].
    /// Filling order: column-major (fill down rows within a column) to make column counts contiguous.
    /// </summary>
    private void CacheAllFormationData()
    {
        cachedFormations.Clear();
        int count = characters.Count;

        foreach (FormationShape shape in Enum.GetValues(typeof(FormationShape)))
        {
            PlayerFormationData data = new PlayerFormationData();

            // -----------------------
            // COMPUTE rows & columns
            // -----------------------
            int rowsLocal = 1;
            int colsLocal = 1;

            int maxR = 0;
            int maxC = 0;

            if (shape == FormationShape.Square)
            {
                maxR = 15;
                maxC = 14;
                rowsLocal = Mathf.Min(maxR, Mathf.CeilToInt(Mathf.Sqrt(count)));
                colsLocal = Mathf.Min(maxC, Mathf.CeilToInt((float)count / rowsLocal));
            }
            else if (shape == FormationShape.Horizontal)
            {
                maxR = 10;
                maxC = 21;

                int idealRows = Mathf.CeilToInt(Mathf.Sqrt(count / 2f));
                int idealCols = idealRows * 2;

                rowsLocal = Mathf.Min(idealRows, maxR);
                colsLocal = Mathf.Min(idealCols, maxC);

                while (rowsLocal < maxR && rowsLocal * colsLocal < count)
                {
                    rowsLocal++;
                    colsLocal = Mathf.Min(rowsLocal * 2, maxC);
                }
            }
            else if (shape == FormationShape.Vertical)
            {
                maxR = 21;
                maxC = 10;

                int idealCols = Mathf.CeilToInt(Mathf.Sqrt(count / 2f));
                int idealRows = idealCols * 2;

                rowsLocal = Mathf.Min(idealRows, maxR);
                colsLocal = Mathf.Min(idealCols, maxC);

                while (colsLocal < maxC && rowsLocal * colsLocal < count)
                {
                    colsLocal++;
                    rowsLocal = Mathf.Min(rowsLocal * 2, maxR);
                }
            }

            // -----------------------
            // PREP arrays
            // -----------------------
            int[] perRow = new int[rowsLocal];
            int[] perCol = new int[colsLocal];
            Vector3[] positions = new Vector3[count];

            // -----------------------
            // FILL character grid
            // -----------------------
            if (shape == FormationShape.Horizontal) // Horizontal -> use row-major
            {
                for (int i = 0; i < count; i++)
                {
                    int col = i % colsLocal;
                    int row = i / colsLocal;

                    perRow[row]++;
                    perCol[col]++;

                    float x = (col - (colsLocal - 1) / 2f) * formationSpacing;
                    float z = (row - (rowsLocal - 1) / 2f) * formationSpacing;

                    positions[i] = new Vector3(x, 0f, -z);
                }
            }
            else // Square or Vertical -> use column-major
            {
                for (int i = 0; i < count; i++)
                {
                    int row = i % rowsLocal;
                    int col = i / rowsLocal;

                    perRow[row]++;
                    perCol[col]++;

                    float x = (col - (colsLocal - 1) / 2f) * formationSpacing;
                    float z = (row - (rowsLocal - 1) / 2f) * formationSpacing;

                    positions[i] = new Vector3(x, 0f, -z);
                }
            }

            // -----------------------
            // COMPUTE rowDepth
            // -----------------------
            List<float> rowDepthLocal = new List<float>();
            float centerRow = (rowsLocal - 1) * 0.5f;

            for (int r = 0; r < rowsLocal; r++)
            {
                float value = (centerRow - r) * formationSpacing;
                rowDepthLocal.Add(value);
            }

            // -----------------------
            // SAVE DATA
            // -----------------------

            data.maxRows = maxR;
            data.maxColumns = maxC;
            data.rows = rowsLocal;
            data.columns = colsLocal;
            data.halfWidth = (colsLocal - 1) * formationSpacing * 0.5f;
            data.halfLength = (rowsLocal - 1) * formationSpacing * 0.5f;
            data.charactersPerRow = perRow;
            data.charactersPerColumn = perCol;
            data.formationPositions = positions;
            data.rowDepth = rowDepthLocal;
            cachedFormations[shape] = data;

            Debug.Log("Caching: " + shape + " | maxR = " + maxR + " | maxC = " + maxC);
        }

       
        UpdateFormationShape(); // After caching all, apply the currently selected
    }



    /// <summary>
    /// 
    /// Apply correct FormationShape cached data
    /// 
    /// </summary>
    private void UpdateFormationShape()
    {
        PlayerFormationData data = GetFormationData(selectedShape);

        rows = data.rows;
        columns = data.columns;
        halfWidth = data.halfWidth;
        halfLength = data.halfLength;
        MaxCols = data.maxColumns;
        MaxRows = data.maxRows;
        Debug.Log("Applying: " + selectedShape + " | loaded maxR = " + data.maxRows);

        charactersPerRow = (int[])data.charactersPerRow.Clone();
        charactersPerColumn = (int[])data.charactersPerColumn.Clone();
        formationPositions = (Vector3[])data.formationPositions.Clone();
        rowDepth = new List<float>(data.rowDepth);

        if (halfWidth != lastHalfWidth || halfLength != lastHalfLength)
        {
            lastHalfWidth = halfWidth;
            lastHalfLength = halfLength;
            OnFormationSizeChanged?.Invoke(halfWidth, halfLength);
        }
    }


    public PlayerFormationData GetFormationData(FormationShape shape)
    {
        if (cachedFormations.TryGetValue(shape, out var data))
            return data;
        return new PlayerFormationData();
    }

    /// <summary>
    /// Moves all characters toward their calculated formation positions smoothly.
    /// Each Character is expected to implement MoveToFormation(Vector3 target).
    /// </summary>
    private void ApplyFormation()
    {
        int n = characters.Count;
        if (formationPositions == null || formationPositions.Length != n) return;

        Vector3 pivotPos = pivot != null ? pivot.position : Vector3.zero;
        for (int i = 0; i < n; i++)
        {
            Character c = characters[i];
            if (c == null) continue;
            Vector3 target = pivotPos + formationPositions[i];
            c.MoveToFormation(target);
        }
    }

}
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
    [SerializeField] private CharacterType activeCharacterType;
    public List<Character> characters = new List<Character>(); // All active characters in group
    public Transform pivot; // Invisible pivot used for input-based movement
    
    // Limits
    public int MaxRows = 15;
    public int MaxCols = 14;

    // Computed formation geometry
    public int columns { get; private set; } = 1;
    public int rows { get; private set; } = 1;
    public float halfWidth { get; private set; } = 0.5f;
    public float halfLength { get; private set; } = 0.5f;

    public float[] rowDepth;

    // per-column / per-row counts (updated on reformation)
    public int[] charactersPerColumn { get; private set; } = new int[0];
    public int[] charactersPerRow { get; private set; } = new int[0];

    // cached formation target positions (world-space offsets from pivot)
    private Vector3[] formationPositions;

    // events
    private float lastHalfWidth;
    private float lastHalfLength;
    public event Action<float, float> OnFormationSizeChanged; // (halfWidth, halfLength)
    public event Action<Character> OnCharacterAdded;
    public event Action<Character> OnCharacterRemoved;

    // managers
    CharacterSpawner characterSpawner;

    // formation mode
    [SerializeField] private FormationShape selectedShape = FormationShape.Square;


    void Start()
    {
        characterSpawner = ServiceLocator.Get<CharacterSpawner>();

        CacheAllFormationData();
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

    public void ChangeAllCharactersTo(CharacterType newType)
    {
        var count = characters.Count;
        var formationOffsets = formationPositions;
        var worldPositions = new Vector3[characters.Count];

        // 1. Release existing characters
        foreach (var c in characters)
            characterSpawner.ReleaseCharacter(c);

        characters.Clear();

        // 2. Spawn new type characters to same positions
        for (int i = 0; i < count; i++)
        {
            worldPositions[i] = pivot.position + formationOffsets[i];
            characterSpawner.SpawnCharacterOfType(newType, worldPositions[i]);
        }

        CacheAllFormationData();
    }


    public void SetActiveCharacterType(CharacterType newType)
    {
        activeCharacterType = newType;

        ChangeAllCharactersTo(newType);
    }

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
    /// Forces recompute of Formation)
    /// </summary>
    public void RebuildFormation()
    {
        CacheAllFormationData();
    }
    #endregion


    [System.Serializable]
    public struct CachedFormationLayout
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
        public float[] rowDepth; // distance of each row from pivot
    }

    public Dictionary<FormationShape, CachedFormationLayout> cachedFormations = new Dictionary<FormationShape, CachedFormationLayout>();


    // -----------------------
    // Struct for FormationCalculation return
    // -----------------------

    public struct FormationCalculator
    {
        public int maxRows;
        public int maxCols;
        public int rows;
        public int cols;
        public int[] perRow;
        public int[] perCol;
        public Vector3[] positions;
        public float[] rowDepth;
        public FormationCalculator(int maxRowsR, int maxColsR, int rowsR, int colsR, int[] perRowR, int[] perColR, Vector3[] positionsR, float[] rowDepthR)
        {
            maxRows = maxRowsR;
            maxCols = maxColsR;
            rows = rowsR;
            cols = colsR;
            perRow = perRowR;
            perCol = perColR;
            positions = positionsR;
            rowDepth = rowDepthR;
        }
    }

    // -----------------------
    // Method to calculate formaiton values (Square)
    // -----------------------
    private FormationCalculator SquareFormationCreator(int count, CharacterType type)
    {
        int maxRows = 15;
        int maxCols = 14;
        int rowsLocal = 1;
        int colsLocal = 1;


        // -----------------------
        // COMPUTE rows & columns
        // -----------------------
        rowsLocal = Mathf.Max(1, Mathf.Min(maxRows, Mathf.CeilToInt(Mathf.Sqrt(Mathf.Max(1, count)))));
        colsLocal = Mathf.Max(1, Mathf.Min(maxCols, Mathf.CeilToInt((float)count / rowsLocal)));

        int[] perRow = new int[rowsLocal];
        int[] perCol = new int[colsLocal];
        Vector3[] positions = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            int row = Mathf.Clamp(i % rowsLocal, 0, rowsLocal - 1);
            int col = Mathf.Clamp(i / rowsLocal, 0, colsLocal - 1);

            perRow[row]++;
            perCol[col]++;

            float x = (col - (colsLocal - 1) / 2f) * type.horizontalSize;
            float z = (row - (rowsLocal - 1) / 2f) * type.verticalSize;

            positions[i] = new Vector3(x, 0f, -z);
        }

        // -----------------------
        // COMPUTE rowDepth (vertical distances)
        // -----------------------
        float[] rowDepthLocal = new float[rowsLocal];
        float centerRow = (rowsLocal - 1) * 0.5f;

        for (int r = 0; r < rowsLocal; r++)
        {
            rowDepthLocal[r] = (centerRow - r) * type.verticalSize;
        }


        return new FormationCalculator(maxRows, maxCols, rowsLocal, colsLocal, perRow, perCol, positions, rowDepthLocal);
    }


    // -----------------------
    // Method to calculate formaiton values (Horizontal)
    // -----------------------
    private FormationCalculator HorizontalFormationCreator(int count, CharacterType type)
    {
        int maxRows = 10;
        int maxCols = 21;

        // -----------------------
        // COMPUTE rows & columns
        // -----------------------

        int idealRows = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(Mathf.Max(1, count / 2f))));
        int idealCols = Mathf.Max(1, idealRows * 2);
        int rowsLocal = 1;
        int colsLocal = 1;

        rowsLocal = Mathf.Min(idealRows, maxRows);
        colsLocal = Mathf.Min(idealCols, maxCols);

        // safety iteration guard: only iterate up to maxR steps
        int safety = 0;
        while (rowsLocal < maxRows && rowsLocal * colsLocal < count && safety < maxRows + 5)
        {
            rowsLocal++;
            colsLocal = Mathf.Min(rowsLocal * 2, maxCols);
            safety++;
        }
        // final clamps
        rowsLocal = Mathf.Clamp(rowsLocal, 1, maxRows);
        colsLocal = Mathf.Clamp(colsLocal, 1, maxCols);

        int[] perRow = new int[rowsLocal];
        int[] perCol = new int[colsLocal];
        Vector3[] positions = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            int col = Mathf.Clamp(i % colsLocal, 0, colsLocal - 1);
            int row = Mathf.Clamp(i / colsLocal, 0, rowsLocal - 1);

            perRow[row]++;
            perCol[col]++;

            float x = (col - (colsLocal - 1) / 2f) * type.horizontalSize;
            float z = (row - (rowsLocal - 1) / 2f) * type.verticalSize;

            positions[i] = new Vector3(x, 0f, -z);
        }

        // -----------------------
        // COMPUTE rowDepth (vertical distances)
        // -----------------------
        float[] rowDepthLocal = new float[rowsLocal];
        float centerRow = (rowsLocal - 1) * 0.5f;

        for (int r = 0; r < rowsLocal; r++)
        {
            rowDepthLocal[r] = (centerRow - r) * type.verticalSize;
        }

        return new FormationCalculator(maxRows, maxCols, rowsLocal, colsLocal, perRow, perCol, positions, rowDepthLocal);
    }


    // -----------------------
    // Method to calculate formaiton values (Vertical)
    // -----------------------
    private FormationCalculator VerticalFormationCreator(int count, CharacterType type)
    {
        int maxRows = 21;
        int maxCols = 10;

        // -----------------------
        // COMPUTE rows & columns
        // -----------------------

        int idealCols = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(Mathf.Max(1, count / 2f))));
        int idealRows = Mathf.Max(1, idealCols * 2);

        int rowsLocal = 1;
        int colsLocal = 1;


        rowsLocal = Mathf.Min(idealRows, maxRows);
        colsLocal = Mathf.Min(idealCols, maxCols);

        int safety = 0;
        while (colsLocal < maxCols && rowsLocal * colsLocal < count && safety < maxCols + 5)
        {
            colsLocal++;
            rowsLocal = Mathf.Min(rowsLocal * 2, maxRows);
            safety++;
        }

        rowsLocal = Mathf.Clamp(rowsLocal, 1, maxRows);
        colsLocal = Mathf.Clamp(colsLocal, 1, maxCols);

        int[] perRow = new int[rowsLocal];
        int[] perCol = new int[colsLocal];
        Vector3[] positions = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            int row = Mathf.Clamp(i % rowsLocal, 0, rowsLocal - 1);
            int col = Mathf.Clamp(i / rowsLocal, 0, colsLocal - 1);

            perRow[row]++;
            perCol[col]++;

            float x = (col - (colsLocal - 1) / 2f) * type.horizontalSize;
            float z = (row - (rowsLocal - 1) / 2f) * type.verticalSize;

            positions[i] = new Vector3(x, 0f, -z);
        }

        // -----------------------
        // COMPUTE rowDepth (vertical distances)
        // -----------------------
        float[] rowDepthLocal = new float[rowsLocal];
        float centerRow = (rowsLocal - 1) * 0.5f;

        for (int r = 0; r < rowsLocal; r++)
        {
            rowDepthLocal[r] = (centerRow - r) * type.verticalSize;
        }


        return new FormationCalculator(maxRows, maxCols, rowsLocal, colsLocal, perRow, perCol, positions, rowDepthLocal);
    }



    /// <summary>
    /// Calculates target positions for each character in a grid formation and updates helper arrays.
    /// Positions are offsets relative to pivot; caller uses pivot.position + formationPositions[i].
    /// Filling order: column-major (fill down rows within a column) to make column counts contiguous.
    /// </summary>
    private void CacheAllFormationData()
    {
        cachedFormations.Clear();
        int count = Mathf.Max(0, characters.Count); // safeGet

        foreach (FormationShape shape in Enum.GetValues(typeof(FormationShape)))
        {

            CachedFormationLayout cachedData = new CachedFormationLayout();

                

            FormationCalculator formation = new FormationCalculator();

            if (shape == FormationShape.Square)
            {
                formation = SquareFormationCreator(count, activeCharacterType);
            }
            else if (shape == FormationShape.Horizontal)
            {
                formation = HorizontalFormationCreator(count, activeCharacterType);
            }
            else if (shape == FormationShape.Vertical)
            {
                formation = VerticalFormationCreator(count, activeCharacterType);
            }

            // -----------------------
            // Set the variables for the "shape"
            // -----------------------
            cachedData.maxRows = formation.maxRows;
            cachedData.maxColumns = formation.maxCols;
            cachedData.rows = formation.rows;
            cachedData.columns = formation.cols;
            cachedData.halfWidth = (formation.cols - 1) * activeCharacterType.horizontalSize * 0.5f;
            cachedData.halfLength = (formation.rows - 1) * activeCharacterType.verticalSize * 0.5f;
            cachedData.charactersPerRow = formation.perRow;
            cachedData.charactersPerColumn = formation.perCol;
            cachedData.formationPositions = formation.positions;
            cachedData.rowDepth = formation.rowDepth;

            // -----------------------
            // Save all data to cache with the "shape"
            // -----------------------
            cachedFormations[shape] = cachedData;
        }

        // After caching all, apply the currently selected
        UpdateFormationShape();
    }




    /// <summary>
    /// 
    /// Apply correct cached data for FormationShape
    /// 
    /// </summary>
    private void UpdateFormationShape()
    {
        CachedFormationLayout data = GetFormationData(selectedShape);

        rows = data.rows;
        columns = data.columns;
        halfWidth = data.halfWidth;
        halfLength = data.halfLength;
        MaxCols = data.maxColumns;
        MaxRows = data.maxRows;

        charactersPerRow = (int[])data.charactersPerRow.Clone();
        charactersPerColumn = (int[])data.charactersPerColumn.Clone();
        formationPositions = (Vector3[])data.formationPositions.Clone();
        rowDepth = (float[])data.rowDepth.Clone();

        if (halfWidth != lastHalfWidth || halfLength != lastHalfLength)
        {
            lastHalfWidth = halfWidth;
            lastHalfLength = halfLength;
            OnFormationSizeChanged?.Invoke(halfWidth, halfLength);
        }
    }


    public CachedFormationLayout GetFormationData(FormationShape shape)
    {
        if (cachedFormations.TryGetValue(shape, out var data))
            return data;
        return new CachedFormationLayout();
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


    /// <summary>
    /// Fills the provided PlayerFormationData using the specified shape and character type/count.
    /// Does NOT touch live cached player formations.
    /// </summary>
    public void GetFormationSnapshot(PlayerFormationData snapshot, FormationShape shape, CharacterType type, int count)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

        FormationCalculator calc = shape switch
        {
            FormationShape.Square => SquareFormationCreator(count, type),
            FormationShape.Horizontal => HorizontalFormationCreator(count, type),
            FormationShape.Vertical => VerticalFormationCreator(count, type),
            _ => throw new ArgumentOutOfRangeException(nameof(shape))
        };

        snapshot.shape = shape;
        snapshot.rows = calc.rows;
        snapshot.cols = calc.cols;
        snapshot.characterCount = count;
        snapshot.rowDepth = calc.rowDepth;
        snapshot.characterType = type;
        snapshot.rowsCharacterCounts = (int[])calc.perRow.Clone();
        snapshot.colsCharacterCounts = (int[])calc.perCol.Clone();
    }

}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a group of player characters acting as a single controllable unit.
/// Responsible for adding/removing characters, maintaining formation,
/// and adjusting formation shape dynamically when the group approaches level boundaries.
/// </summary>
public class CharacterGroupManager : MonoBehaviour
{
    [Header("Character Settings")]
    public List<Character> characters = new List<Character>(); // All active characters in group
    public Transform pivot; // Invisible pivot used for input-based movement
    public float formationSpacing = 3f; // Minimum distance between characters

    private float lastHalfWidth;
    private float lastHalfLength;
    public event Action<float, float> OnFormationSizeChanged; // to send (halfWidth, halfLength) to the PivotController who is registered to this event
    public event Action<Character> OnCharacterAdded;
    public event Action<Character> OnCharacterRemoved;

    // Cached array for calculated formation target positions
    private Vector3[] formationPositions;


    void Start()
    {
        UpdateFormationPositions();
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


    /// <summary>
    /// Adds a character to the group and triggers reformation.
    /// </summary>
    public void AddCharacter(Character character)
    {
        if (character == null || characters.Contains(character)) return;

        characters.Add(character);
        OnCharacterAdded?.Invoke(character);
        UpdateFormationPositions();
    }

    /// <summary>
    /// Removes a character from the group and triggers reformation.
    /// </summary>
    public void RemoveCharacter(Character character)
    {
        if (character == null || !characters.Contains(character)) return;

        characters.Remove(character);
        OnCharacterRemoved?.Invoke(character);
        UpdateFormationPositions();
    }

    /// <summary>
    /// Calculates target positions for each character in a grid formation
    /// </summary>
    private void UpdateFormationPositions()
    {
        if (characters.Count == 0) return;

        if (formationPositions == null || formationPositions.Length != characters.Count)
            formationPositions = new Vector3[characters.Count];

        int maxRows = 15;
        int maxCols = 25;
        int rows = Mathf.Min(maxRows, Mathf.CeilToInt(Mathf.Sqrt(characters.Count)));
        int cols = Mathf.Min(maxCols, Mathf.CeilToInt((float)characters.Count / rows));

        float halfWidth = (cols - 1) * formationSpacing * 0.5f;
        float halfLength = (rows - 1) * formationSpacing * 0.5f;

        for (int i = 0; i < characters.Count; i++)
        {
            int row = i % rows;
            int col = i / rows;

            float x = (col - (cols - 1) / 2f) * formationSpacing;
            float z = (row - (rows - 1) / 2f) * formationSpacing;

            formationPositions[i] = new Vector3(x, 0, -z);
        }

        if (halfWidth != lastHalfWidth || halfLength != lastHalfLength)
        {
            lastHalfWidth = halfWidth;
            lastHalfLength = halfLength;
            OnFormationSizeChanged?.Invoke(halfWidth, halfLength);
        }

    }





    /// <summary>
    /// Moves all characters toward their calculated formation positions smoothly.
    /// </summary>
    private void ApplyFormation()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            Character c = characters[i];
            if (c == null) continue;

            Vector3 target = pivot.position + formationPositions[i];
            c.MoveToFormation(target);
        }
    }
}

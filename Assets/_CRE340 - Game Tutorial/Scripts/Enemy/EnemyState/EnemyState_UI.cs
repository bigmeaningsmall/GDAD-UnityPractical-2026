using UnityEngine;
using TMPro;

// Debug label showing which state the parent enemy is in.
public class EnemyState_UI : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;

    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();

        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }
    }

    private void OnEnable()
    {
        if (enemy != null)
        {
            enemy.OnStateChanged += ShowState;
        }
    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.OnStateChanged -= ShowState;
        }
    }

    // Runs when the enemy changes state, not every frame
    private void ShowState(string stateName)
    {
        if (textMesh != null)
        {
            textMesh.text = stateName;
        }
    }
}
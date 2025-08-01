using UnityEngine;

[ExecuteAlways]
public class BuildZoneGizmos : MonoBehaviour
{
    public Vector2 size = Vector2.one * 5f;
    public Color fillColor = new Color(0f, 1f, 0f, 0.2f);
    public Color borderColor = new Color(0f, 1f, 0f, 1f);

    void OnDrawGizmos()
    {
        Gizmos.color = fillColor;
        Gizmos.DrawCube(transform.position, size);
        Gizmos.color = borderColor;
        Gizmos.DrawWireCube(transform.position, size);
    }
}

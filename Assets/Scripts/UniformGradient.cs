using TMPro;
using UnityEngine;

public class UniformGradient : MonoBehaviour
{
    public Color startColor = Color.blue;
    public Color endColor = Color.red;

    void Update()
    {
        TMP_Text textMesh = GetComponent<TMP_Text>();
        textMesh.ForceMeshUpdate(); // Important to update the mesh

        var vertexColors = textMesh.textInfo.meshInfo[0].colors32;
        var characterCount = textMesh.textInfo.characterCount;

        for (int i = 0; i < characterCount; i++)
        {
            int vertexIndex = textMesh.textInfo.characterInfo[i].vertexIndex;
            
            if (textMesh.textInfo.characterInfo[i].isVisible)
            {
                Color color = Color.Lerp(startColor, endColor, (float)i / (characterCount - 1));
                vertexColors[vertexIndex + 0] = color;
                vertexColors[vertexIndex + 1] = color;
                vertexColors[vertexIndex + 2] = color;
                vertexColors[vertexIndex + 3] = color;
            }
        }

        // Update the mesh with the new colors
        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}

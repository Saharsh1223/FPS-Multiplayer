using UnityEngine;
using System.Collections;

public class VectorLerp : MonoBehaviour
{
	// Example usage:
	// Vector3 startVector = new Vector3(0, 0, 0);
	// Vector3 endVector = new Vector3(10, 5, 3);
	// float duration = 2.0f;
	// StartCoroutine(Lerp(startVector, endVector, duration));

	// Global method for lerping between two vectors over a specified duration
	public static IEnumerator Lerp(Vector3 startVector, Vector3 endVector, float duration)
	{
		float elapsedTime = 0f;

		while (elapsedTime < duration)
		{
			// Calculate the interpolation factor (lerp factor) based on elapsed time and duration
			float t = Mathf.Clamp01(elapsedTime / duration);

			// Perform linear interpolation between start and end vectors
			Vector3 lerpedVector = Vector3.Lerp(startVector, endVector, t);

			// You can use the lerpedVector for whatever you need (e.g., move an object)
			// For example, transform.position = lerpedVector;

			// Increment elapsed time based on the frame rate
			elapsedTime += Time.deltaTime;

			// Wait for the next frame
			yield return null;
		}

		// Ensure that the final position is exactly the endVector
		// This is useful to handle floating-point imprecision
		//transform.position = endVector;
	}
}

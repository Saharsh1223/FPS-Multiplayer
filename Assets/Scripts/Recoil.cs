using UnityEngine;
using Photon.Pun;

public class Recoil : MonoBehaviourPun
{
    private Vector3 currentRotation;
    private Vector3 targetRotation;
    
    [HideInInspector] public float recoilX;
    [HideInInspector] public float recoilY;
    [HideInInspector] public float recoilZ;
    
    [HideInInspector] public float snappiness;
    [HideInInspector] public float returnSpeed;

    private void Update()
    {
        // Smoothly return to the original rotation
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }
    
    public void RecoilFire()
    {
        if (photonView.IsMine) // Apply recoil only if this PhotonView belongs to the local player
        {
            // Apply recoil locally
            ApplyRecoil();
        }
    }

    private void ApplyRecoil()
    {
        Vector3 rotationVector = new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
        targetRotation += rotationVector;
    }
}

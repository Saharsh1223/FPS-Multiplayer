using UnityEngine;
using Photon.Pun;

public class GrapplingRope : MonoBehaviour
{
    private Spring spring;
    private LineRenderer lr;
    private Vector3 currentGrapplePosition;
    private Vector3[] ropePositions;
    public GrapplingGun grapplingGun;
    public int quality;
    public float damper;
    public float strength;
    public float velocity;
    public float waveCount;
    public float waveHeight;
    public AnimationCurve affectCurve;
    public PlayerSetup ropeSyncManager;
    public PhotonView photonView;

    private float syncInterval = 0.02f; // Adjust based on your needs
    private float lastSyncTime;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        spring = new Spring();
        spring.SetTarget(0);
        ropePositions = new Vector3[quality + 1];
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            spring.SetVelocity(velocity);
        }
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            bool isGrappling = grapplingGun.IsGrappling();
            if (isGrappling)
            {
                DrawRope();
            }
            else
            {
                DisableRope();
            }

            if (Time.time - lastSyncTime >= syncInterval)
            {
                SyncRope();
                lastSyncTime = Time.time;
            }
            
            ropeSyncManager.SetRopeVisibilityForAll(isGrappling);
        }
        else
        {
            UpdateRemoteRope();
        }
    }

    private void DrawRope()
    {
        if (!grapplingGun.IsGrappling())
        {
            currentGrapplePosition = grapplingGun.gunTip.position;
            spring.Reset();
            if (lr.positionCount > 0)
                lr.positionCount = 0;
            return;
        }

        if (lr.positionCount == 0)
        {
            spring.SetVelocity(velocity);
            lr.positionCount = quality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.UpdateMethod(Time.deltaTime);

        var grapplePoint = grapplingGun.GetGrapplePoint();
        var gunTipPosition = grapplingGun.gunTip.position;
        var up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

        for (var i = 0; i < quality + 1; i++)
        {
            var delta = i / (float)quality;
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
                         affectCurve.Evaluate(delta);

            ropePositions[i] = Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset;
            lr.SetPosition(i, ropePositions[i]);
        }
    }

    private void SyncRope()
    {
        if (ropeSyncManager != null)
        {
            Vector3[] compressedPositions = CompressRopePositions(ropePositions);
            ropeSyncManager.SendRopePositions(compressedPositions);
        }
    }

    private Vector3[] CompressRopePositions(Vector3[] positions)
    {
        // Example compression: Reduce precision or only send a subset
        return positions; // Modify this to compress as needed
    }

    public void UpdateRopePositions(Vector3[] positions)
    {
        ropePositions = positions;
        lr.positionCount = positions.Length;

        for (int i = 0; i < positions.Length; i++)
        {
            lr.SetPosition(i, positions[i]);
        }
    }

    private void UpdateRemoteRope()
    {
        if (ropePositions != null && ropePositions.Length > 0)
        {
            if (lr.positionCount != ropePositions.Length)
            {
                lr.positionCount = ropePositions.Length;
            }

            for (int i = 0; i < ropePositions.Length; i++)
            {
                if (lr.GetPosition(i) != ropePositions[i])
                {
                    lr.SetPosition(i, ropePositions[i]);
                }
            }
        }
    }

    public void SetRopeVisibility(bool isVisible)
    {
        lr.enabled = isVisible;
    }

    private void DisableRope()
    {
        if (lr.positionCount > 0)
        {
            lr.positionCount = 0;
        }
    }
}

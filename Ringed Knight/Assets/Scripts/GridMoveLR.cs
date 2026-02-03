using System.Collections;
using UnityEngine;

public class GridMoveLR : MonoBehaviour
{
    [Header("Grid")]
    public float cellSize = 1f;
    public float moveDuration = 0.08f;
    public LayerMask obstacleMask;
    public Vector2 checkBoxSize = new Vector2(0.6f, 0.9f);
    public Transform checkOrigin;

    [Header("Flip (simple)")]
    public SpriteRenderer spriteRenderer; 

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip stepClip;

    [Header("Dust (Prefab)")]
    public ParticleSystem dustPrefab; 
    public Vector3 dustOffset = new Vector3(0f, -0.45f, 0f);
    public int dustBurstCount = 14;

    private ParticleSystem dustInstance;
    private bool isMoving;
    private int facing = 1; // 1 derecha, -1 izquierda

    void Awake()
    {
        if (!checkOrigin) checkOrigin = transform;
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        if (dustPrefab)
        {
            dustInstance = Instantiate(dustPrefab, transform);
            dustInstance.transform.localPosition = Vector3.zero;
            dustInstance.transform.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        if (isMoving) return;

        int dir = GetInputDir();
        if (dir == 0) return;

        facing = dir;

        if (spriteRenderer) spriteRenderer.flipX = (facing == -1);

        Vector3 target = transform.position + Vector3.right * dir * cellSize;

        if (IsBlocked(target)) return;

        StartCoroutine(MoveTo(target));
    }

    int GetInputDir()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) return -1;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) return 1;

        if (Input.GetMouseButtonDown(0))
            return (Input.mousePosition.x < Screen.width * 0.5f) ? -1 : 1;

        return 0;
    }

    bool IsBlocked(Vector3 targetPos)
    {
        Vector2 center = new Vector2(targetPos.x, checkOrigin.position.y);
        return Physics2D.OverlapBox(center, checkBoxSize, 0f, obstacleMask);
    }

    IEnumerator MoveTo(Vector3 target)
    {
        isMoving = true;

        PlayStep();
        PlayDust();

        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.001f, moveDuration);
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }

    void PlayStep()
    {
        if (!audioSource || !stepClip) return;
        audioSource.PlayOneShot(stepClip);
    }

    void PlayDust()
    {
        if (!dustInstance) return;

        Vector3 off = dustOffset;
        off.x = Mathf.Abs(off.x) * -facing;
        off.z = 0f;

        dustInstance.transform.position = transform.position + off;

        dustInstance.transform.rotation = (facing == 1)
            ? Quaternion.Euler(0, 0, 0)
            : Quaternion.Euler(0, 180f, 0);

        // Emitir puff
        dustInstance.Emit(dustBurstCount);
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!checkOrigin) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3(checkOrigin.position.x, checkOrigin.position.y, 0f),
            new Vector3(checkBoxSize.x, checkBoxSize.y, 0f)
        );
    }
#endif
}

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource effectAudioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip GoldCollect;
    [SerializeField] private AudioClip hitClip;
    public void PlayShootSound()
    {
        if (effectAudioSource != null && shootClip != null)
        {
            effectAudioSource.PlayOneShot(shootClip);
        }
        else
        {
            Debug.LogWarning("AudioSource or ShootClip is not assigned in EnemyAudioManager.");
        }
    }
    public void PlayGoldCollectSound()
    {
        if (effectAudioSource != null && GoldCollect != null)
        {
            effectAudioSource.PlayOneShot(GoldCollect);
        }
        else
        {
            Debug.LogWarning("AudioSource or GoldCollect is not assigned in EnemyAudioManager.");
        }
    }
    public void PlayHitSound()
    {
        if (effectAudioSource != null && hitClip != null)
        {
            effectAudioSource.PlayOneShot(hitClip);
        }
        else
        {
            Debug.LogWarning("AudioSource or HitClip is not assigned in EnemyAudioManager.");
        }
    }
}

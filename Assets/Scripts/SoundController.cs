using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController Instance;

    [Space(10), Header("SFX")] 
    public AudioClip LevelFanfarre;

    [Space(10), Header("Munch SFXs")]
    public AudioClip Munch1SFX;
    public AudioClip Munch2SFX;

    [Space(10), Header("Siren SFXs")] 
    public AudioClip Siren1SFX;
    public AudioClip Siren2SFX;
    public AudioClip Siren3SFX;
    public AudioClip Siren4SFX;
    public AudioClip Siren5SFX;

    [Space(10), Header("Energizer SFXs")] 
    public AudioClip EnergizerSFX;

    [Space(10), Header("Fruit SFXs")] 
    public AudioClip FruitSFX;

    [Space(10), Header("Ghost SFXs")]
    public AudioClip GhostSFX;
    public AudioClip RetreatingSFX;

    [Space(10), Header("Audio Sources")]
    public AudioSource MusicSource;
    public AudioSource SFXSource;
    
    private AudioClip _currentMunch;
    private AudioClip _currentSiren;

    private AudioClip[] _sirenClips;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _sirenClips = new[] { Siren1SFX, Siren2SFX, Siren3SFX, Siren4SFX, Siren5SFX };
        
        _currentMunch = Munch1SFX;
        _currentSiren = _sirenClips[0];

        PlaySirenMusic();
    }

    public void PlayLevelFanfarre()
    {
        MusicSource.PlayOneShot(LevelFanfarre);
    }

    public void PlaySirenMusic()
    {
        MusicSource.clip = _currentSiren;
        MusicSource.Play();
    }

    public void PlayPelletEatenSFX()
    {
        _currentMunch = _currentMunch == Munch1SFX ? Munch2SFX : Munch1SFX;
        SFXSource.PlayOneShot(_currentMunch);
    }

    public void PlayFruitEatenSFX()
    {
        SFXSource.PlayOneShot(FruitSFX);
    }

    public void PlayGhostEatenSFX()
    {
        SFXSource.PlayOneShot(GhostSFX);
    }

    public void PlayEnergizerMusic()
    {
        StopAllCoroutines();

        StartCoroutine(nameof(PlayEnergizerMusicCoroutine));
    }

    private IEnumerator PlayEnergizerMusicCoroutine()
    {
        MusicSource.clip = EnergizerSFX;
        MusicSource.Play();

        GameController.Instance.ChangeFrightenedModeTo(true);

        yield return new WaitForSeconds(LevelController.Instance.GetCurrentLevel().NormalEnergizerTime * 0.75f);

        GameController.Instance.IsEnergizedTimeEnding = true;

        yield return new WaitForSeconds(LevelController.Instance.GetCurrentLevel().NormalEnergizerTime * 0.25f);

        _currentSiren = _sirenClips[GameController.Instance.GetEnergizersConsumed()];
        PlaySirenMusic();

        GameController.Instance.IsEnergizedTimeEnding = false;

        GameController.Instance.ChangeFrightenedModeTo(false);
    }
}

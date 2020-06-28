using System;
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
    
    [Space(10), Header("Dying SFXs")]
    public AudioClip Dying1SFX;
    public AudioClip Dying2SFX;

    [Space(10), Header("Audio Sources")]
    public AudioSource MusicSource;
    public AudioSource SFXSource;
    public AudioSource AuxSource;

    private AudioClip _currentMunch;
    private AudioClip _currentSiren;

    private AudioClip[] _sirenClips;

    private bool _defeated;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _sirenClips = new[] { Siren1SFX, Siren2SFX, Siren3SFX, Siren4SFX, Siren5SFX };
        
        _currentMunch = Munch1SFX;
        _currentSiren = _sirenClips[0];

        PlayLevelFanfarre();
    }

    public void PlayLevelFanfarre()
    {
        StartCoroutine(nameof(PlayLevelFanfarreCoroutine));
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
        PlayRetreatingMusic();
    }

    public void PlayEnergizerMusic()
    {
        StopAllCoroutines();

        StartCoroutine(nameof(PlayEnergizerMusicCoroutine));
    }

    public void PlayRetreatingMusic()
    {
        StartCoroutine(nameof(PlayRetreatingMusicCoroutine));
    }

    public void PlayDyingMusic()
    {
        if (_defeated) return;
        _defeated = true;

        StopAllCoroutines();

        StartCoroutine(nameof(PlayDyingMusicCoroutine));
    }

    private IEnumerator PlayLevelFanfarreCoroutine()
    {
        Time.timeScale = 0f;

        AuxSource.clip = LevelFanfarre;
        AuxSource.Play();
        
        yield return new WaitForSecondsRealtime(LevelFanfarre.length + 1f);

        AuxSource.Stop();
        AuxSource.clip = null;

        Time.timeScale = 1f;

        PlaySirenMusic();
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

    private IEnumerator PlayRetreatingMusicCoroutine()
    {
        AuxSource.clip = RetreatingSFX;
        AuxSource.Play();

        yield return new WaitForSeconds(3);

        AuxSource.clip = null;
        AuxSource.Stop();
    }

    private IEnumerator PlayDyingMusicCoroutine()
    {
        AuxSource.clip = Dying1SFX;
        AuxSource.Play();

        yield return new WaitForSecondsRealtime(Dying1SFX.length - 1f);

        AuxSource.Stop();
        AuxSource.clip = Dying2SFX;
        AuxSource.Play();

        yield return new WaitForSecondsRealtime(Dying2SFX.length);

        AuxSource.clip = null;
        AuxSource.Stop();
    }
}

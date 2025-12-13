using Godot;
using System;

public partial class SoundManager : Node
{
	public float defaultMusicDb = -20f;
	public float defaultFxDb = -15f;

	
	// Musiques principales en fond de jeu 
	[Export] public AudioStreamPlayer menuThemeMusic; 
	[Export] public AudioStreamPlayer introJeuThemeMusic;
	[Export] public AudioStreamPlayer mainThemeMusic;
	[Export] public AudioStreamPlayer newIslandThemeMusic;
	[Export] public AudioStreamPlayer gameOverThemeMusic;
	
	public void PlayMenuTheme() => menuThemeMusic.Play();
	public void PlayIntroJeuTheme() => introJeuThemeMusic.Play();
	public void PlayMainTheme() => mainThemeMusic.Play();
	public void PlayNewIslandTheme() => newIslandThemeMusic.Play();
	public void PlayGameOverTheme() => gameOverThemeMusic.Play();
	
	public void StopMenuTheme() => menuThemeMusic.Stop();
	public void StopIntroJeuTheme() => introJeuThemeMusic.Stop();
	public void StopMainTheme() => mainThemeMusic.Stop();
	public void StopNewIslandTheme() => newIslandThemeMusic.Stop();
	public void StopGameOverTheme() => gameOverThemeMusic.Stop();
	
	// le paramètre là dessous est la variable qui représente l'ASP
	public void EcouterExclusivement(AudioStreamPlayer audioPlayer)
	{
		StopMenuTheme();
		StopIntroJeuTheme();
		StopMainTheme();
		StopNewIslandTheme();
		StopGameOverTheme();
		
		audioPlayer.Play();
	}
	
	public void StopAllMusic()
	{
		StopMenuTheme();
		StopIntroJeuTheme();
		StopMainTheme();
		StopNewIslandTheme();
		StopGameOverTheme();
	}
	
	
	//========================================================================
	
	// Bruitages et sons
	[Export] public AudioStreamPlayer mobDamagedAudio;  
	[Export] public AudioStreamPlayer mobDamaged2Audio;
	[Export] public AudioStreamPlayer playerDamagedAudio;
	[Export] public AudioStreamPlayer deathMobAudio;
	[Export] public AudioStreamPlayer chestOpenAudio;
	[Export] public AudioStreamPlayer coinPickAudio;
	[Export] public AudioStreamPlayer heartPickAudio;
	[Export] public AudioStreamPlayer teleportAudio;
	[Export] public AudioStreamPlayer boutonMenuAudio;
	[Export] public AudioStreamPlayer bombExplosionAudio;
	[Export] public AudioStreamPlayer highJumpAudio;
	[Export] public AudioStreamPlayer jumpAudio;
	[Export] public AudioStreamPlayer secretItemFoundAudio;
	[Export] public AudioStreamPlayer gameOverAudio;


	public void PlayMobDamaged() => mobDamagedAudio.Play();
	public void PlayMobDamaged2() => mobDamaged2Audio.Play();
	public void PlayPlayerDamaged() => playerDamagedAudio.Play();
	public void PlayDeathMob() => deathMobAudio.Play();
	public void PlayChestOpen() => chestOpenAudio.Play();
	public void PlayCoinPick() => coinPickAudio.Play();
	public void PlayHeartPick() => heartPickAudio.Play();
	public void PlayTeleport() => teleportAudio.Play();
	public void PlayBoutonMenu() => boutonMenuAudio.Play();
	public void PlayBombExplosion() => bombExplosionAudio.Play();
	public void PlayHighJump() => highJumpAudio.Play();
	public void PlayJumpAudio() => jumpAudio.Play();
	public void PlaySecretItemFound() => secretItemFoundAudio.Play();
	public void PlayGameOver() => gameOverAudio.Play();


	public void SetMusicVolume(float volumeLineaire)
	{
		float db = Mathf.LinearToDb(volumeLineaire);
		GD.Print("le volume de la musique est de : " + db + "dB");
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), db);
	}

	public void SetFXVolume(float volumeLineaire)
	{
		float db = Mathf.LinearToDb(volumeLineaire);
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("FX"), db);
	}
	
	public void MuteMusic(bool mute)
	{
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), mute);
	}
	
	public void MuteFX(bool mute)
	{
		AudioServer.SetBusMute(AudioServer.GetBusIndex("FX"), mute);
	}
	
	public override void _Ready()
{
	// Applique le volume de base
	AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), defaultMusicDb);
	AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("FX"), defaultFxDb);

	// Et s’assure que rien n'est mute au lancement
	AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), false);
	AudioServer.SetBusMute(AudioServer.GetBusIndex("FX"), false);
}

	
	
}

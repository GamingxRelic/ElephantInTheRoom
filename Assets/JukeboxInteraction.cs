using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JukeboxInteraction : MonoBehaviour
{
    [System.Serializable]
    public struct Songs
    {
        public string id;
        public AudioSource player;
        public PickableObject music_disc_object;
    }

    [SerializeField] private List<Songs> songs = new List<Songs>();
    private Songs current_song;
    [SerializeField] private Songs default_song;

    private void Start()
    {
        current_song = default_song;
    }

    public void OnInteract()
    {
        if (PlayerController.instance.held_object != null) // If player is holding an item
        {
            for (int i = 0; i < songs.Count; i++) // Check if player is holding a music disc
            {
                if (PlayerController.instance.held_object.id == songs[i].music_disc_object.id)
                {
                    // Remove disc from player's hand
                    GameObject obj = PlayerController.instance.held_object.gameObject;
                    obj.transform.SetParent(null);
                    Destroy(obj);
                    PlayerController.instance.held_object = null;

                    // If disc is already playing, give player back disc.
                    if(current_song.id != default_song.id)
                    {
                        GameObject new_disc = Instantiate(current_song.music_disc_object.gameObject);
                        new_disc.transform.SetParent(PlayerController.instance.hand_point);
                        new_disc.transform.position = PlayerController.instance.hand_point.position;
                        PlayerController.instance.held_object = new_disc.GetComponent<PickableObject>();
                    }

                    // Play the song
                    PlaySong(songs[i]);

                    // Trigger goal
                    ChecklistHandler.instance.TriggerGoal("play_music_disc");

                    return;
                }
            }

            // Otherwise, check if jukebox is playing a music disc song and if so, give it to the player
            PlayerController.instance.DropObject();
            GameObject disc = Instantiate(current_song.music_disc_object.gameObject);
            disc.transform.SetParent(PlayerController.instance.hand_point);
            disc.transform.position = PlayerController.instance.hand_point.position;
            PlayerController.instance.held_object = disc.GetComponent<PickableObject>();
        }
        else if(current_song.id != default_song.id)
        {
            // Give player the music disc
            GameObject disc = Instantiate(current_song.music_disc_object.gameObject);
            disc.transform.SetParent(PlayerController.instance.hand_point);
            disc.transform.localPosition = Vector3.zero;
            PlayerController.instance.held_object = current_song.music_disc_object;
            PlayerController.instance.held_object = disc.GetComponent<PickableObject>();

            // Play the new song
            PlaySong(default_song);
        }

    }

    private void PlaySong(Songs song)
    {
        current_song.player.Stop();
        current_song = song;
        current_song.player.Play();
    }
}


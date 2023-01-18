using UnityEngine;

namespace SEE.Audio
{
    /// <summary>
    /// Defines an interface for an audio managing framework.
    /// </summary>
    public interface IAudioManager
    {
        /// <summary>
        /// Mutes all music.
        /// </summary>
        void MuteMusic();

        /// <summary>
        /// Increases the music volume.
        /// </summary>
        void IncreaseMusicVolume();

        /// <summary>
        /// Decreases the music volume.
        /// </summary>
        void DecreaseMusicVolume();

        /// <summary>
        /// Unmute the music.
        /// </summary>
        void UnmuteMusic();

        /// <summary>
        /// Mute all sound effects.
        /// </summary>
        void MuteSoundEffects();

        /// <summary>
        /// Increase sound effects volume.
        /// </summary>
        void IncreaseSoundEffectVolume();

        /// <summary>
        /// Decrease sound effects volume.
        /// </summary>
        void DecreaseSoundEffectVolume();

        /// <summary>
        /// Unmute all sound effects.
        /// </summary>
        void UnmuteSoundEffects();

        /// <summary>
        /// Pause the currently playing music.
        /// </summary>
        void PauseMusic();

        /// <summary>
        /// Resume music player if it was paused previously.
        /// </summary>
        void ResumeMusic();

        /// <summary>
        /// Change the currently playing music based on the new game state.
        /// </summary>
        void GameStateChanged();

        /// <summary>
        /// Queue a sound effect without specifying the object which the sound is originating from.
        /// The GameObject used is the player object itself (ambient sound rather than directional sound is used).
        /// </summary>
        /// <param name="soundEffect">The sound effect that should be played.</param>
        public void QueueSoundEffect(SoundEffect soundEffect);

        /// <summary>
        /// Adds a sound effect to the sound effect queue.
        /// </summary>
        /// <param name="soundEffect">The sound effect that should be added to the sound effect queue.</param>
        /// <param name="sourceObject">The GameObject where the sound originates from.</param>
        public void QueueSoundEffect(SoundEffect soundEffect, GameObject sourceObject);

        /// <summary>
        /// Adds a sound effect to the sound effect queue while checking,
        /// if the sound effect was passed from a multiplayer connected player,
        /// or from the local game instance (to prevent endless sound effect loops).
        /// </summary>
        /// <param name="soundEffect">The sound effect that should be added to the sound effect queue.</param>
        /// <param name="sourceObject">The GameObject where the sound originated from.</param>
        /// <param name="networkAction">Whether the sound effect originated from the local unity instance.</param>
        public void QueueSoundEffect(SoundEffect soundEffect, GameObject sourceObject, bool networkAction);

        /// <summary>
        /// Defines abstract names for different sound effects that can be played in-game.
        /// </summary>
        enum SoundEffect
        {
            /// <summary>
            /// A simple click sound. 
            /// </summary>
            CLICK_SOUND,
            /// <summary>
            /// Sound for dropping objects.
            /// </summary>
            DROP_SOUND,
            /// <summary>
            /// Confirmation click sound.
            /// </summary>
            OKAY_SOUND,
            /// <summary>
            /// Sound for picking up objects.
            /// </summary>
            PICKUP_SOUND,
            /// <summary>
            /// Sound for creating a new edge.
            /// </summary>
            NEW_EDGE_SOUND,
            /// <summary>
            /// Sound for creating a new node.
            /// </summary>
            NEW_NODE_SOUND,
            /// <summary>
            /// Player walking sound.
            /// </summary>
            WALKING_SOUND,
            /// <summary>
            /// Declined click sound.
            /// </summary>
            CANCEL_SOUND,
            /// <summary>
            /// Drawing sound.
            /// </summary>
            SCRIBBLE,
            /// <summary>
            /// Sound for hovering over objects.
            /// </summary>
            HOVER_SOUND
        }

        /// <summary>
        /// Defines abstract names for different music tracks that can be played in-game.
        /// </summary>
        enum Music
        {
            /// <summary>
            /// The lobby music.
            /// </summary>
            LOBBY_MUSIC 
        }
    }
}

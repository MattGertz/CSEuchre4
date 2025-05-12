using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CSEuchre4
{    /// <summary>
    /// Helper class for playing sounds synchronously in the Euchre game
    /// </summary>
    public static class EuchreSoundPlayer
    {        // Control the pacing of the game with these constants
        private static readonly int ShuffleDelay = 300; // ms delay between shuffle sounds
        private static readonly int CardPlayDelay = 150; // ms delay after playing card sounds
        private static readonly int AnimationDelay = 50; // ms delay to ensure animation starts before sound
        private static readonly int ConsecutiveCardDelay = 300; // ms delay between consecutive card plays
        
        // Track when the last card sound was played to ensure proper spacing between consecutive cards
        private static DateTime _lastCardSoundTime = DateTime.MinValue;
        
        /// <summary>
        /// Plays a sound synchronously from a stream
        /// </summary>
        public static void PlaySoundSync(UnmanagedMemoryStream soundStream)
        {
            if (soundStream == null)
                return;
                
            try
            {
                // Reset stream position
                soundStream.Position = 0;
                
                using (var player = new System.Media.SoundPlayer(soundStream))
                {
                    // PlaySync blocks until the sound has finished playing
                    player.PlaySync();
                }
            }
            catch (Exception)
            {
                // Silently handle any sound playback errors
            }
        }
        
        /// <summary>
        /// Plays a sound asynchronously from a stream
        /// </summary>
        public static void PlaySoundAsync(UnmanagedMemoryStream soundStream)
        {
            if (soundStream == null)
                return;
            
            try
            {
                // Reset stream position
                soundStream.Position = 0;
                
                using (var player = new System.Media.SoundPlayer(soundStream))
                {
                    // Play asynchronously
                    player.Play();
                }
            }
            catch (Exception)
            {
                // Silently handle any sound playback errors
            }
        }
        
        /// <summary>
        /// Plays the shuffle sound multiple times with a delay between each play
        /// </summary>
        public static void PlayShuffleSequence(UnmanagedMemoryStream shuffleSound, int count)
        {
            if (shuffleSound == null || count <= 0) 
                return;
                
            for (int i = 0; i < count; i++)
            {
                PlaySoundSync(shuffleSound);
                
                // Add a small delay between shuffle sounds
                if (i < count - 1)
                {
                    Thread.Sleep(ShuffleDelay);
                }
            }
        }
        
        /// <summary>
        /// Plays a card sound and adds a small delay for pacing
        /// </summary>
        public static void PlayCardSoundWithPacing(UnmanagedMemoryStream cardSound)
        {
            PlaySoundSync(cardSound);
            
            // Add a small delay after a card is played for better pacing
            Thread.Sleep(CardPlayDelay);
        }
          /// <summary>
        /// Plays a card sound with proper timing to synchronize with the UI animation
        /// </summary>
        public static void PlayCardSoundWithAnimation(UnmanagedMemoryStream cardSound, UIElement element)
        {
            if (element == null || cardSound == null)
                return;
                
            // Ensure minimum spacing between consecutive card plays
            TimeSpan timeSinceLastSound = DateTime.Now - _lastCardSoundTime;
            if (timeSinceLastSound.TotalMilliseconds < ConsecutiveCardDelay)
            {
                int waitTime = ConsecutiveCardDelay - (int)timeSinceLastSound.TotalMilliseconds;
                if (waitTime > 0)
                {
                    Thread.Sleep(waitTime);
                }
            }
            
            // First make sure the UI element is refreshed and visible
            element.Dispatcher.Invoke(() => {
                // Force layout update to ensure animations start properly
                element.UpdateLayout();
            }, DispatcherPriority.Render);
            
            // Give a small delay to let the animation start
            Thread.Sleep(AnimationDelay);
            
            // Now play the sound - it will be better synchronized with the animation
            PlaySoundSync(cardSound);
            
            // Update the last card sound time to maintain proper spacing
            _lastCardSoundTime = DateTime.Now;
        }        
        
        /// <summary>
        /// Plays a card sound with proper timing specifically when playing a trick with multiple cards.
        /// This is a simplified synchronous implementation that blocks the UI thread until complete.
        /// </summary>
        /// <param name="cardSound">Card sound to play</param>
        /// <param name="element">UI element associated with this card</param>
        /// <param name="isSequence">True if this is part of a sequence of card plays</param>
        /// <param name="positionInSequence">Position in the sequence (0-3, where 0 is first card)</param>
        public static void PlayCardSoundForTrick(UnmanagedMemoryStream cardSound, UIElement element, bool isSequence, int positionInSequence)
        {
            if (element == null)
                return;
              
            // First make sure the UI element is refreshed and visible
            element.Dispatcher.Invoke(() => {
                // Force layout update to ensure animations start properly
                element.UpdateLayout();
            }, DispatcherPriority.Render);
            
            // Give a small delay to let the animation start
            Thread.Sleep(AnimationDelay);
            
            if (cardSound != null)
            {
                // Play sound synchronously - this blocks the UI thread until complete
                PlaySoundSync(cardSound);
            }
            else
            {
                // If no sound (sound turned off), sleep for a similar duration
                Thread.Sleep(CardPlayDelay);
            }
        }
    }
}

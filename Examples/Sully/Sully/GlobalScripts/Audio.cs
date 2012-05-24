using System;
using System.Collections.Generic;
using System.IO;

using XNAVERGE;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Sully {
    public struct AudioBank {
        public WaveBank wave;
        public SoundBank sound;
    }

    public partial class _ {        
        public static void PlayMusic(string s) {
            if (String.IsNullOrEmpty(s)) { StopMusic(); }
            Cue new_cue = sg.music_bank.sound.GetCue(s);            
            if (sg.current_music != null) {
                if (sg.current_music.Name == new_cue.Name) { return; }
                sg.current_music.Stop(AudioStopOptions.Immediate); 
            }
            new_cue.Play();
            sg.current_music = new_cue;
        }

        public static void StopMusic() {
            if (sg.current_music != null) {
                sg.current_music.Stop(AudioStopOptions.Immediate);
            }
        }
    }
}

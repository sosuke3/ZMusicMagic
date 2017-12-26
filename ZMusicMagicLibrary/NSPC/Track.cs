using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary.NSPC
{
    public class Track
    {
        public int Address { get; set; }
        public byte[] RawTrackData { get; set; } = new byte[0]; // this might contain extra data past the end of the actual track data

        public enum Command : byte
        {
            [Description("Return")]
            _00_Return = 0x0,

            [Description("C 1")]
            _80_C1 = 0x80,
            [Description("C# 1")]
            _81_Db1 = 0x81,
            [Description("D 1")]
            _82_D1 = 0x82,
            [Description("D# 1")]
            _83_Eb1 = 0x83,
            [Description("E 1")]
            _84_E1 = 0x84,
            [Description("F 1")]
            _85_F1 = 0x85,
            [Description("F# 1")]
            _86_Gb1 = 0x86,
            [Description("G 1")]
            _87_G1 = 0x87,
            [Description("G# 1")]
            _88_Ab1 = 0x88,
            [Description("A 1")]
            _89_A1 = 0x89,
            [Description("A# 1")]
            _8A_Bb1 = 0x8A,
            [Description("B 1")]
            _8B_B1 = 0x8B,

            [Description("C 2")]
            _8C_C2 = 0x8C,
            [Description("C# 2")]
            _8D_Db2 = 0x8D,
            [Description("D 2")]
            _8E_D2 = 0x8E,
            [Description("D# 2")]
            _8F_Eb2 = 0x8F,
            [Description("E 2")]
            _90_E2 = 0x90,
            [Description("F 2")]
            _91_F2 = 0x91,
            [Description("F# 2")]
            _92_Gb2 = 0x92,
            [Description("G 2")]
            _93_G2 = 0x93,
            [Description("G# 2")]
            _94_Ab2 = 0x94,
            [Description("A 2")]
            _95_A2 = 0x95,
            [Description("A# 2")]
            _96_Bb2 = 0x96,
            [Description("B 2")]
            _97_B2 = 0x97,

            [Description("C 3")]
            _98_C3 = 0x98,
            [Description("C# 3")]
            _99_Db3 = 0x99,
            [Description("D 3")]
            _9A_D3 = 0x9A,
            [Description("D# 3")]
            _9B_Eb3 = 0x9B,
            [Description("E 3")]
            _9C_E3 = 0x9C,
            [Description("F 3")]
            _9D_F3 = 0x9D,
            [Description("F# 3")]
            _9E_Gb3 = 0x9E,
            [Description("G 3")]
            _9F_G3 = 0x9F,
            [Description("G# 3")]
            _A0_Ab3 = 0xA0,
            [Description("A 3")]
            _A1_A3 = 0xA1,
            [Description("A# 3")]
            _A2_Bb3 = 0xA2,
            [Description("B 3")]
            _A3_B3 = 0xA3,

            [Description("C 4")]
            _A4_C4 = 0xA4,
            [Description("C# 4")]
            _A5_Db4 = 0xA5,
            [Description("D 4")]
            _A6_D4 = 0xA6,
            [Description("D# 4")]
            _A7_Eb4 = 0xA7,
            [Description("E 4")]
            _A8_E4 = 0xA8,
            [Description("F 4")]
            _A9_F4 = 0xA9,
            [Description("F# 4")]
            _AA_Gb4 = 0xAA,
            [Description("G 4")]
            _AB_G4 = 0xAB,
            [Description("G# 4")]
            _AC_Ab4 = 0xAC,
            [Description("A 4")]
            _AD_A4 = 0xAD,
            [Description("A# 4")]
            _AE_Bb4 = 0xAE,
            [Description("B 4")]
            _AF_B4 = 0xAF,

            [Description("C 5")]
            _B0_C5 = 0xB0,
            [Description("C# 5")]
            _B1_Db5 = 0xB1,
            [Description("D 5")]
            _B2_D5 = 0xB2,
            [Description("D# 5")]
            _B3_Eb5 = 0xB3,
            [Description("E 5")]
            _B4_E5 = 0xB4,
            [Description("F 5")]
            _B5_F5 = 0xB5,
            [Description("F# 5")]
            _B6_Gb5 = 0xB6,
            [Description("G 5")]
            _B7_G5 = 0xB7,
            [Description("G# 5")]
            _B8_Ab5 = 0xB8,
            [Description("A 5")]
            _B9_A5 = 0xB9,
            [Description("A# 5")]
            _BA_Bb5 = 0xBA,
            [Description("B 5")]
            _BB_B5 = 0xBB,

            [Description("C 6")]
            _BC_C6 = 0xBC,
            [Description("C# 6")]
            _BD_Db6 = 0xBD,
            [Description("D 6")]
            _BE_D6 = 0xBE,
            [Description("D# 6")]
            _BF_Eb6 = 0xBF,
            [Description("E 6")]
            _C0_E6 = 0xC0,
            [Description("F 6")]
            _C1_F6 = 0xC1,
            [Description("F# 6")]
            _C2_Gb6 = 0xC2,
            [Description("G 6")]
            _C3_G6 = 0xC3,
            [Description("G# 6")]
            _C4_Ab6 = 0xC4,
            [Description("A 6")]
            _C5_A6 = 0xC5,
            [Description("A# 6")]
            _C6_Bb6 = 0xC6,
            [Description("B 6")]
            _C7_B6 = 0xC7,

            [Description("Tie")]
            _C8_Tie = 0xC8,
            [Description("Rest")]
            _C9_Rest = 0xC9,

            // apparently these are not used by ALttP
            _CA_Percussion0 = 0xCA,
            _CB_Percussion1 = 0xCB,
            _CC_Percussion2 = 0xCC,
            _CD_Percussion3 = 0xCD,
            _CE_Percussion4 = 0xCE,
            _CF_Percussion5 = 0xCF,
            _D0_Percussion6 = 0xD0,
            _D1_Percussion7 = 0xD1,
            _D2_Percussion8 = 0xD2,
            _D3_Percussion9 = 0xD3,
            _D4_Percussion10 = 0xD4,
            _D5_Percussion11 = 0xD5,
            _D6_Percussion12 = 0xD6,
            _D7_Percussion13 = 0xD7,
            _D8_Percussion14 = 0xD8,
            _D9_Percussion15 = 0xD9,
            _DA_Percussion16 = 0xDA,
            _DB_Percussion17 = 0xDB,
            _DC_Percussion18 = 0xDC,
            _DD_Percussion19 = 0xDD,
            _DE_Percussion20 = 0xDE,
            _DF_Percussion21 = 0xDF,

            [Description("Set Instrument")]
            _E0_SetInstrument = 0xE0,       // 1 parameter

            [Description("Pan")]
            _E1_Pan = 0xE1,                 // 1 parameter
            [Description("Pan Fade")]
            _E2_PanFade = 0xE2,             // 2 parameters

            [Description("Vibrato")]
            _E3_VibratoOn = 0xE3,           // 3 parameters
            [Description("Vibrato Off")]
            _E4_VibratoOff = 0xE4,          // 0 parameters

            [Description("Master Volume")]
            _E5_MasterVolume = 0xE5,        // 1 parameter
            [Description("Master Volume Fade")]
            _E6_MasterVolumeFade = 0xE6,    // 2 parameters

            [Description("Tempo")]
            _E7_Tempo = 0xE7,               // 1 parameter
            [Description("Tempo Fade")]
            _E8_TempoFade = 0xE8,           // 2 parameters

            [Description("Global Transpose")]
            _E9_GlobalTranspose = 0xE9,     // 1 parameter
            [Description("Channel Transpose")]
            _EA_PerChannelTranspose = 0xEA, // 1 parameter

            [Description("Tremolo")]
            _EB_TremoloOn = 0xEB,           // 3 parameters
            [Description("Tremolo Off")]
            _EC_TremoloOff = 0xEC,          // 0 parameters

            [Description("Channel Volume")]
            _ED_ChannelVolume = 0xED,       // 1 parameter
            [Description("Channel Volume Fade")]
            _EE_ChannelVolumeFade = 0xEE,   // 2 parameters

            [Description("Call Loop")]
            _EF_CallLoop = 0xEF,            // 3 parameters

            [Description("Vibrato Fade")]
            _F0_VibratoFade = 0xF0,         // 1 parameter

            [Description("Pitch Envelope To")]
            _F1_PitchEnvelopeTo = 0xF1,     // 3 parameters
            [Description("Pitch Envelope From")]
            _F2_PitchEnvelopeFrom = 0xF2,   // 3 parameters
            [Description("Pitch Envelope Off")]
            _F3_PitchEnvelopeOff = 0xF3,    // 0 parameters

            [Description("Tuning Adjustment")]
            _F4_Tuning = 0xF4,              // 1 parameter

            [Description("Echo Volume")]
            _F5_EchoVolume = 0xF5,          // 3 parameters
            [Description("Echo Off")]
            _F6_EchoOff = 0xF6,             // 0 parameters
            [Description("Echo Parameters")]
            _F7_EchoParameters = 0xF7,      // 3 parameters
            [Description("Echo Volume Fade")]
            _F8_EchoVolumeFade = 0xF8,      // 3 parameters

            [Description("Pitch Slide")]
            _F9_PitchSlide = 0xF9,          // 3 parameters

            [Description("Percussion Patch Base")] // it's odd that this is used, but percussion commands are never used.
            _FA_PercussionPatchBase = 0xFA, // 1 parameter

            //// commands that appear in other versions of the N-SPC engine
            _FB_CommandFB = 0xFB,           // 2 parameters
            _FC_CommandFC = 0xFC,           // 0 parameters
            _FD_CommandFD = 0xFD,           // 0 parameters
            _FE_CommandFE = 0xFE,           // 0 parameters
            _FF_CommandFF = 0xFF            // 0 parameters
        }

        public readonly static Dictionary<Command, int> CommandParameterCount = new Dictionary<Command, int>()
        {
            { Command._E0_SetInstrument, 1 },
            { Command._E1_Pan, 1 },
            { Command._E2_PanFade, 2 },
            { Command._E3_VibratoOn, 3 },
            { Command._E4_VibratoOff, 0 },
            { Command._E5_MasterVolume, 1 },
            { Command._E6_MasterVolumeFade, 2 },
            { Command._E7_Tempo, 1 },
            { Command._E8_TempoFade, 2 },
            { Command._E9_GlobalTranspose, 1 },
            { Command._EA_PerChannelTranspose, 1 },
            { Command._EB_TremoloOn, 3 },
            { Command._EC_TremoloOff, 0 },
            { Command._ED_ChannelVolume, 1 },
            { Command._EE_ChannelVolumeFade, 2 },
            { Command._EF_CallLoop, 3 },
            { Command._F0_VibratoFade, 1 },
            { Command._F1_PitchEnvelopeTo, 3 },
            { Command._F2_PitchEnvelopeFrom, 3 },
            { Command._F3_PitchEnvelopeOff, 0 },
            { Command._F4_Tuning, 1 },
            { Command._F5_EchoVolume, 3 },
            { Command._F6_EchoOff, 0 },
            { Command._F7_EchoParameters, 3 },
            { Command._F8_EchoVolumeFade, 3 },
            { Command._F9_PitchSlide, 3 },
            { Command._FA_PercussionPatchBase, 1 },
            { Command._FB_CommandFB, 2 },
            { Command._FC_CommandFC, 0 },
            { Command._FD_CommandFD, 0 },
            { Command._FE_CommandFE, 0 },
            { Command._FF_CommandFF, 0 },
        };

    }
}

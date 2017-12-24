using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary
{
    public class Rom
    {
        public SongCollection OverworldSongs { get; set; }
        public SongCollection IndoorSongs { get; set; }
        public SongCollection EndingSongs { get; set; }

        byte[] romData;
        public byte[] RomData { get { return romData; } }

        public void LoadRom(string filename)
        {
            romData = File.ReadAllBytes(filename);

            if(false == isValidRom())
            {
                throw new Exception("Invalid rom file");
            }

            OverworldSongs = new SongCollection();
            LoadSongCollection(OverworldSongs, 0x91C, 0x918, 0x914); // vanilla should be $1A9EF5 -> 0xD1EF5

            IndoorSongs = new SongCollection();
            LoadSongCollection(IndoorSongs, 0x92E, 0x92A, 0x926); // vanilla should be $1B8000 -> 0xD8000

            EndingSongs = new SongCollection();
            LoadSongCollection(EndingSongs, 0x93A, 0x936, 0x932); // vanilla should be $1AD380 -> 0xD5380

            var baseNspc = new NSPC.NSPC();
            int baseNspcAddress = LoadNspcAddress(0x90A, 0x906, 0x902); // vanilla should be $198000 -> $C8000
            baseNspc.LoadRom(this, baseNspcAddress);

            var overworldNspc = new NSPC.NSPC();
            int overworldNspcAddress = LoadNspcAddress(0x91C, 0x918, 0x914); // vanilla should be $1A9EF5 -> 0xD1EF5
            overworldNspc.LoadRom(this, overworldNspcAddress);

            var indoorNspc = new NSPC.NSPC();
            int indoorNspcAddress = LoadNspcAddress(0x92E, 0x92A, 0x926); // vanilla should be $1B8000 -> 0xD8000
            indoorNspc.LoadRom(this, indoorNspcAddress);

            var endingNspc = new NSPC.NSPC();
            int endingNspcAddress = LoadNspcAddress(0x93A, 0x936, 0x932); // vanilla should be $1AD380 -> 0xD5380
            endingNspc.LoadRom(this, endingNspcAddress);
        }

        private void LoadSongCollection(SongCollection songCollection, int bankByteAddress, int highByteAddress, int lowByteAddress)
        {
            if(songCollection == null)
            {
                throw new ArgumentException("songCollection");
            }

            var songBankAddress = LoadNspcAddress(bankByteAddress, highByteAddress, lowByteAddress);

            Debug.WriteLine($"Song Bank starting address: {songBankAddress.ToString("X")}");
            // load chunks
        }

        int LoadNspcAddress(int bankByteAddress, int highByteAddress, int lowByteAddress)
        {
            var snesBank = romData[Utilities.SnesToPCAddress(bankByteAddress)];
            var snesHigh = romData[Utilities.SnesToPCAddress(highByteAddress)];
            var snesLow = romData[Utilities.SnesToPCAddress(lowByteAddress)];

            return Utilities.SnesToPCAddress(snesBank, snesHigh, snesLow);
        }

        #region Helper Methods and Properties
        bool isValidRom()
        {
            if(romData.Length < 0x10000)
            {
                return false;
            }

            if(IsVanillaRom)
            {
                return true;
            }

            if(romData.Length < 0x20000)
            {
                return false;
            }

            if(IsRandomizerRom)
            {
                return true;
            }

            return false;
        }

        public bool IsVanillaRom
        {
            get
            {
                return IsVanillaJPRom || IsVanillaUSRom;
            }
        }

        public bool IsVanillaJPRom
        {
            get
            {
                // vanilla JP
                // ZELDANODENSETSU
                if (romData[0x7FC0] == 'Z'
                    && romData[0x7FC1] == 'E'
                    && romData[0x7FC2] == 'L'
                    && romData[0x7FC3] == 'D'
                    && romData[0x7FC4] == 'A'
                    && romData[0x7FC5] == 'N'
                    && romData[0x7FC6] == 'O'
                    && romData[0x7FC7] == 'D'
                    && romData[0x7FC8] == 'E'
                    && romData[0x7FC9] == 'N'
                    && romData[0x7FCA] == 'S'
                    && romData[0x7FCB] == 'E'
                    && romData[0x7FCC] == 'T'
                    && romData[0x7FCD] == 'S'
                    && romData[0x7FCE] == 'U'
                    && romData[0x7FCF] == 0x20)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsVanillaUSRom
        {
            get
            {
                // vanilla US
                //THE LEGEND OF ZELDA
                if (romData[0x7FC0] == 'T'
                    && romData[0x7FC1] == 'H'
                    && romData[0x7FC2] == 'E'
                    && romData[0x7FC3] == ' '
                    && romData[0x7FC4] == 'L'
                    && romData[0x7FC5] == 'E'
                    && romData[0x7FC6] == 'G'
                    && romData[0x7FC7] == 'E'
                    && romData[0x7FC8] == 'N'
                    && romData[0x7FC9] == 'D'
                    && romData[0x7FCA] == ' '
                    && romData[0x7FCB] == 'O'
                    && romData[0x7FCC] == 'F'
                    && romData[0x7FCD] == ' '
                    && romData[0x7FCE] == 'Z'
                    && romData[0x7FCF] == 'E'
                    && romData[0x7FD0] == 'L'
                    && romData[0x7FD1] == 'D'
                    && romData[0x7FD2] == 'A'
                    && romData[0x7FD3] == 0x20)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsRandomizerRom
        {
            get
            {
                return IsItemRandomizerRom || IsEntranceRandomizerRom;
            }
        }

        public bool IsItemRandomizerRom
        {
            get
            {
                // item randomizer
                if (romData[0x7FC0] == 0x56 && romData[0x7FC1] == 0x54)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsEntranceRandomizerRom
        {
            get
            {
                // entrance randomizer
                if (romData[0x7FC0] == 0x45 && romData[0x7FC1] == 0x52)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsRaceRom
        {
            get
            {
                if (romData[0x180213] == 0x01 && romData[0x180214] == 0x00)
                {
                    return true;
                }

                return false;
            }
        }

        public void ExpandRom()
        {
            Array.Resize(ref this.romData, 0x400000); // 4MB
            this.romData[0x7FD7] = 0x0C; // update header length
            //SetPatchBytes(0x7FD7, 1);
        }

        public byte[] GetDataChunk(int startingAddress, int length)
        {
            var output = new byte[length];
            Array.Copy(this.romData, startingAddress, output, 0, length);
            return output;
        }

        public void WriteDataChunk(int startingAddress, byte[] data, int length = -1)
        {
            if (length < 0)
            {
                length = data.Length;
            }
            Array.Copy(data, 0, this.romData, startingAddress, length);
        }

        public void WriteRom(Stream fs)
        {
            UpdateChecksum();

            fs.Write(this.romData, 0, this.romData.Length);
        }

        public void UpdateChecksum()
        {
            int checksum = 0;

            // remove old checksum
            romData[Constants.ChecksumComplimentAddress] = 0xFF; // compliment
            romData[Constants.ChecksumComplimentAddress + 1] = 0xFF; // compliment
            romData[Constants.ChecksumAddress] = 0x00; // checksum
            romData[Constants.ChecksumAddress + 1] = 0x00; // checksum

            foreach (byte b in romData)
            {
                checksum += b;
            }

            checksum &= 0xFFFF;
            romData[Constants.ChecksumAddress] = (byte)(checksum & 0xFF);
            romData[Constants.ChecksumAddress + 1] = (byte)((checksum >> 8) & 0xFF);

            int compliment = checksum ^ 0xFFFF; // compliment
            romData[Constants.ChecksumComplimentAddress] = (byte)(compliment & 0xFF);
            romData[Constants.ChecksumComplimentAddress + 1] = (byte)((compliment >> 8) & 0xFF);
        }

        public void ReadFileStreamIntoRom(FileStream f, int address, int length)
        {
            f.Read(romData, address, length);
        }
        #endregion
    }
    /*
    ; *$901-$912 LOCAL
    Sound_LoadIntroSongBank: ; this should be called load driver. also loads overworld bank
    {
        ; $00[3] = $198000, which is $C8000 in Rom
        LDA.b #$00 : STA $00 ; $902 = #$00 ; low
        LDA.b #$80 : STA $01 ; $906 = #$80 ; high
        LDA.b #$19 : STA $02 ; $90A = #$19 ; bank
        
        SEI
        
        JSR Sound_LoadSongBank
        
        CLI
        
        RTS
    }

; ==============================================================================

    ; *$0913-$0924 LONG
    Sound_LoadLightWorldSongBank: ; this should be overworld not lightworld
    {
        ; $00[3] = $1A9EF5, which is $D1EF5 in Rom
        LDA.b #$F5 : STA $00 ; $914 = #$F5 ; low
        LDA.b #$9E : STA $01 ; $918 = #$9E ; high
        LDA.b #$1A           ; $91C = #$1A ; bank
    
    .do_load
    
        STA $02
        
        SEI
        
        JSR Sound_LoadSongBank
        
        CLI
        
        RTL
    
    ; *$0925-$0930 ALTERNATE ENTRY POINT
    shared Sound_LoadIndoorSongBank:
    
        ; $00[3] = $1B8000, which is $D8000 in rom
        LDA.b #$00 : STA $00 ; $926 = #$00 ; low
        LDA.b #$80 : STA $01 ; $92A = #$80 ; high
        LDA.b #$1B           ; $92E = #$1B ; bank
        
        BRA .do_load
    
    ; *$0931-$093C ALTERNATE ENTRY POINT
    shared Sound_LoadEndingSongBank:
    
        ; $00[3] = $1AD380, which is $D5380 in rom
        LDA.b #$80 : STA $00 ; $932 = #$80 ; low
        LDA.b #$D3 : STA $01 ; $936 = #$D3 ; high
        LDA.b #$1A           ; $93A = #$1A ; bank
        
        BRA .do_load
    }
     */
}

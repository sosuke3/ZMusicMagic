using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNES_SPC
{
    public static class opcodes
    {
        public static void Trace(int opcode, int a, int x, int y, int sp, int dp, int pc, SNES_SPC spc, byte[] ram)
        {
            var op = OpCodeTable[opcode];
            int psw = 0;
            spc.GET_PSW(ref psw);
            //nvpbhiZC
            string n = ((psw & SNES_SPC.n80) >> 7) == 1 ? "N" : "n";
            string v = ((psw & SNES_SPC.v40) >> 6) == 1 ? "V" : "v";
            string p = ((psw & SNES_SPC.p20) >> 5) == 1 ? "P" : "p";
            string b = ((psw & SNES_SPC.b10) >> 4) == 1 ? "B" : "b";
            string h = ((psw & SNES_SPC.h08) >> 3) == 1 ? "H" : "h";
            string i = ((psw & SNES_SPC.i04) >> 2) == 1 ? "I" : "i";
            string z = ((psw & SNES_SPC.z02) >> 1) == 1 ? "Z" : "z";
            string c = ((psw & SNES_SPC.c01)) == 1 ? "C" : "c";
            string nvpbhizc = $"{n}{v}{p}{b}{h}{i}{z}{c}";
            sp -= 1; // quick maths
            Debug.WriteLine($"..{pc.ToString("x4")} {op.GetInstructionWithValues(a, x, y, pc+1, dp, sp, ram)} A:{a.ToString("x2")} X:{x.ToString("x2")} Y:{y.ToString("x2")} SP:{sp.ToString("x4")} YA:{y.ToString("x2")}{a.ToString("x2")} {nvpbhizc}");
        }
        public static Dictionary<int, OpCode> OpCodeTable { get; private set; } = new Dictionary<int, OpCode>()
        {
            { 0x99, new OpCode(0x99, "ADC   (X),(Y)     ", "adc   (x),(y)", 1, " 5 ", "(X) = (X)+(Y)+C                  ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x88, new OpCode(0x88, "ADC   A,#i        ", "adc   a,#${0}", 2, " 2 ", "A = A+i+C                        ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x86, new OpCode(0x86, "ADC   A,(X)       ", "adc   a,(x)", 1, " 3 ", "A = A+(X)+C                      ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x97, new OpCode(0x97, "ADC   A,[d]+Y     ", "adc   a,($0{0})+y", 2, " 6 ", "A = A+([d]+Y)+C                  ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x87, new OpCode(0x87, "ADC   A,[d+X]     ", "adc   a,($0{0}+x)", 2, " 6 ", "A = A+([d+X])+C                  ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x84, new OpCode(0x84, "ADC   A,d         ", "adc   a,$0{0}", 2, " 3 ", "A = A+(d)+C                      ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x94, new OpCode(0x94, "ADC   A,d+X       ", "adc   a,$0{0}+x", 2, " 4 ", "A = A+(d+X)+C                    ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x85, new OpCode(0x85, "ADC   A,!a        ", "adc   a,${1}{0}", 3, " 4 ", "A = A+(a)+C                      ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x95, new OpCode(0x95, "ADC   A,!a+X      ", "adc   a,${1}{0}+x", 3, " 5 ", "A = A+(a+X)+C                    ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x96, new OpCode(0x96, "ADC   A,!a+Y      ", "adc   a,${1}{0}+y", 3, " 5 ", "A = A+(a+Y)+C                    ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x89, new OpCode(0x89, "ADC   dd,ds       ", "adc   $0{1},$0{0}", 3, " 6 ", "(dd) = (dd)+(d)+C                ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x98, new OpCode(0x98, "ADC   d,#i        ", "adc   $0{1},#${0}", 3, " 5 ", "(d) = (d)+i+C                    ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x7A, new OpCode(0x7A, "ADDW  YA,d        ", "addw  ya,$0{0}", 2, " 5 ", "YA  = YA + (d), H on high byte   ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x39, new OpCode(0x39, "AND   (X),(Y)     ", "and   (x),(y)", 1, " 5 ", "(X) = (X) & (Y)                  ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x28, new OpCode(0x28, "AND   A,#i        ", "and   a,#${0}", 2, " 2 ", "A = A & i                        ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x26, new OpCode(0x26, "AND   A,(X)       ", "and   a,(x)", 1, " 3 ", "A = A & (X)                      ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x37, new OpCode(0x37, "AND   A,[d]+Y     ", "and   a,($0{0})+y", 2, " 6 ", "A = A & ([d]+Y)                  ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x27, new OpCode(0x27, "AND   A,[d+X]     ", "and   a,($0{0}+x)", 2, " 6 ", "A = A & ([d+X])                  ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x24, new OpCode(0x24, "AND   A,d         ", "and   a,$0{0}", 2, " 3 ", "A = A & (d)                      ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x34, new OpCode(0x34, "AND   A,d+X       ", "and   a,$0{0}+x", 2, " 4 ", "A = A & (d+X)                    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x25, new OpCode(0x25, "AND   A,!a        ", "and   a,${1}{0}", 3, " 4 ", "A = A & (a)                      ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x35, new OpCode(0x35, "AND   A,!a+X      ", "and   a,${1}{0}+x", 3, " 5 ", "A = A & (a+X)                    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x36, new OpCode(0x36, "AND   A,!a+Y      ", "and   a,${1}{0}+y", 3, " 5 ", "A = A & (a+Y)                    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x29, new OpCode(0x29, "AND   dd,ds       ", "and   $0{1},$0{0}", 3, " 6 ", "(dd) = (dd) & (ds)               ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x38, new OpCode(0x38, "AND   d,#i        ", "and   $0{1},#${0}", 3, " 5 ", "(d) = (d) & i                    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x6A, new OpCode(0x6A, "AND1  C,/m.b      ", "and1  c,~{1}{0}", 3, " 4 ", "C = C & ~(m.b)                   ", ".", ".", ".", ".", ".", ".", ".", "C") },
            { 0x4A, new OpCode(0x4A, "AND1  C,m.b       ", "and1  c,{1}{0}", 3, " 4 ", "C = C & (m.b)                    ", ".", ".", ".", ".", ".", ".", ".", "C") },
            { 0x1C, new OpCode(0x1C, "ASL   A           ", "asl   a", 1, " 2 ", "Left shift A: high->C, 0->low    ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x0B, new OpCode(0x0B, "ASL   d           ", "asl   $0{0}", 2, " 4 ", "Left shift (d) as above          ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x1B, new OpCode(0x1B, "ASL   d+X         ", "asl   $0{0}+x", 2, " 5 ", "Left shift (d+X) as above        ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x0C, new OpCode(0x0C, "ASL   !a          ", "asl   ${1}{0}", 3, " 5 ", "Left shift (a) as above          ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x13, new OpCode(0x13, "BBC   d.0,r       ", "bbc0  $0{0},${3}", 3, "5/7", "PC+=r  if d.0 == 0               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x33, new OpCode(0x33, "BBC   d.1,r       ", "bbc1  $0{0},${3}", 3, "5/7", "PC+=r  if d.1 == 0               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x53, new OpCode(0x53, "BBC   d.2,r       ", "bbc2  $0{0},${3}", 3, "5/7", "PC+=r  if d.2 == 0               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x73, new OpCode(0x73, "BBC   d.3,r       ", "bbc3  $0{0},${3}", 3, "5/7", "PC+=r  if d.3 == 0               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x93, new OpCode(0x93, "BBC   d.4,r       ", "bbc4  $0{0},${3}", 3, "5/7", "PC+=r  if d.4 == 0               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xB3, new OpCode(0xB3, "BBC   d.5,r       ", "bbc5  $0{0},${3}", 3, "5/7", "PC+=r  if d.5 == 0               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xD3, new OpCode(0xD3, "BBC   d.6,r       ", "bbc6  $0{0},${3}", 3, "5/7", "PC+=r  if d.6 == 0               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xF3, new OpCode(0xF3, "BBC   d.7,r       ", "bbc7  $0{0},${3}", 3, "5/7", "PC+=r  if d.7 == 0               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x03, new OpCode(0x03, "BBS   d.0,r       ", "bbs0  $0{0},${3}", 3, "5/7", "PC+=r  if d.0 == 1               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x23, new OpCode(0x23, "BBS   d.1,r       ", "bbs1  $0{0},${3}", 3, "5/7", "PC+=r  if d.1 == 1               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x43, new OpCode(0x43, "BBS   d.2,r       ", "bbs2  $0{0},${3}", 3, "5/7", "PC+=r  if d.2 == 1               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x63, new OpCode(0x63, "BBS   d.3,r       ", "bbs3  $0{0},${3}", 3, "5/7", "PC+=r  if d.3 == 1               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x83, new OpCode(0x83, "BBS   d.4,r       ", "bbs4  $0{0},${3}", 3, "5/7", "PC+=r  if d.4 == 1               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xA3, new OpCode(0xA3, "BBS   d.5,r       ", "bbs5  $0{0},${3}", 3, "5/7", "PC+=r  if d.5 == 1               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xC3, new OpCode(0xC3, "BBS   d.6,r       ", "bbs6  $0{0},${3}", 3, "5/7", "PC+=r  if d.6 == 1               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xE3, new OpCode(0xE3, "BBS   d.7,r       ", "bbs7  $0{0},${3}", 3, "5/7", "PC+=r  if d.7 == 1               ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x90, new OpCode(0x90, "BCC   r           ", "bcc   ${2}", 2, "2/4", "PC+=r  if C == 0                 ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xB0, new OpCode(0xB0, "BCS   r           ", "bcs   ${2}", 2, "2/4", "PC+=r  if C == 1                 ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xF0, new OpCode(0xF0, "BEQ   r           ", "beq   ${2}", 2, "2/4", "PC+=r  if Z == 1                 ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x30, new OpCode(0x30, "BMI   r           ", "bmi   ${2}", 2, "2/4", "PC+=r  if N == 1                 ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xD0, new OpCode(0xD0, "BNE   r           ", "bne   ${2}", 2, "2/4", "PC+=r  if Z == 0                 ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x10, new OpCode(0x10, "BPL   r           ", "bpl   ${2}", 2, "2/4", "PC+=r  if N == 0                 ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x50, new OpCode(0x50, "BVC   r           ", "bvc   ${2}", 2, "2/4", "PC+=r  if V == 0                 ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x70, new OpCode(0x70, "BVS   r           ", "bvs   ${2}", 2, "2/4", "PC+=r  if V == 1                 ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x2F, new OpCode(0x2F, "BRA   r           ", "bra   ${2}", 2, " 4 ", "PC+=r                            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x0F, new OpCode(0x0F, "BRK               ", "brk", 1, " 8 ", "Push PC and Flags, PC = [$FFDE]  ", ".", ".", ".", "1", ".", "0", ".", ".") },
            { 0x3F, new OpCode(0x3F, "CALL  !a          ", "call  ${1}{0}", 3, " 8 ", "(SP--)=PCh, (SP--)=PCl, PC=a     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xDE, new OpCode(0xDE, "CBNE  d+X,r       ", "cbne  $0{0}+x,${3}", 3, "6/8", "CMP A, (d+X) then BNE            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x2E, new OpCode(0x2E, "CBNE  d,r         ", "cbne  $0{0},${3}", 3, "5/7", "CMP A, (d) then BNE              ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x12, new OpCode(0x12, "CLR1  d.0         ", "clr0  $0{0}", 2, " 4 ", "d.0 = 0                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x32, new OpCode(0x32, "CLR1  d.1         ", "clr1  $0{0}", 2, " 4 ", "d.1 = 0                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x52, new OpCode(0x52, "CLR1  d.2         ", "clr2  $0{0}", 2, " 4 ", "d.2 = 0                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x72, new OpCode(0x72, "CLR1  d.3         ", "clr3  $0{0}", 2, " 4 ", "d.3 = 0                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x92, new OpCode(0x92, "CLR1  d.4         ", "clr4  $0{0}", 2, " 4 ", "d.4 = 0                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xB2, new OpCode(0xB2, "CLR1  d.5         ", "clr5  $0{0}", 2, " 4 ", "d.5 = 0                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xD2, new OpCode(0xD2, "CLR1  d.6         ", "clr6  $0{0}", 2, " 4 ", "d.6 = 0                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xF2, new OpCode(0xF2, "CLR1  d.7         ", "clr7  $0{0}", 2, " 4 ", "d.7 = 0                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x60, new OpCode(0x60, "CLRC              ", "clrc", 1, " 2 ", "C = 0                            ", ".", ".", ".", ".", ".", ".", ".", "0") },
            { 0x20, new OpCode(0x20, "CLRP              ", "clrp", 1, " 2 ", "P = 0                            ", ".", ".", "0", ".", ".", ".", ".", ".") },
            { 0xE0, new OpCode(0xE0, "CLRV              ", "clrv", 1, " 2 ", "V = 0, H = 0                     ", ".", "0", ".", ".", "0", ".", ".", ".") },
            { 0x79, new OpCode(0x79, "CMP   (X),(Y)     ", "cmp   (x),(y)", 1, " 5 ", "(X) - (Y)                        ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x68, new OpCode(0x68, "CMP   A,#i        ", "cmp   a,#${0}", 2, " 2 ", "A - i                            ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x66, new OpCode(0x66, "CMP   A,(X)       ", "cmp   a,(x)", 1, " 3 ", "A - (X)                          ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x77, new OpCode(0x77, "CMP   A,[d]+Y     ", "cmp   a,($0{0})+y", 2, " 6 ", "A - ([d]+Y)                      ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x67, new OpCode(0x67, "CMP   A,[d+X]     ", "cmp   a,($0{0}+x)", 2, " 6 ", "A - ([d+X])                      ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x64, new OpCode(0x64, "CMP   A,d         ", "cmp   a,$0{0}", 2, " 3 ", "A - (d)                          ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x74, new OpCode(0x74, "CMP   A,d+X       ", "cmp   a,$0{0}+x", 2, " 4 ", "A - (d+X)                        ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x65, new OpCode(0x65, "CMP   A,!a        ", "cmp   a,${1}{0}", 3, " 4 ", "A - (a)                          ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x75, new OpCode(0x75, "CMP   A,!a+X      ", "cmp   a,${1}{0}+x", 3, " 5 ", "A - (a+X)                        ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x76, new OpCode(0x76, "CMP   A,!a+Y      ", "cmp   a,${1}{0}+y", 3, " 5 ", "A - (a+Y)                        ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0xC8, new OpCode(0xC8, "CMP   X,#i        ", "cmp   x,#${0}", 2, " 2 ", "X - i                            ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x3E, new OpCode(0x3E, "CMP   X,d         ", "cmp   x,$0{0}", 2, " 3 ", "X - (d)                          ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x1E, new OpCode(0x1E, "CMP   X,!a        ", "cmp   x,${1}{0}", 3, " 4 ", "X - (a)                          ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0xAD, new OpCode(0xAD, "CMP   Y,#i        ", "cmp   y,#${0}", 2, " 2 ", "Y - i                            ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x7E, new OpCode(0x7E, "CMP   Y,d         ", "cmp   y,$0{0}", 2, " 3 ", "Y - (d)                          ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x5E, new OpCode(0x5E, "CMP   Y,!a        ", "cmp   y,${1}{0}", 3, " 4 ", "Y - (a)                          ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x69, new OpCode(0x69, "CMP   dd,ds       ", "cmp   $0{1},$0{0}", 3, " 6 ", "(dd) - (ds)                      ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x78, new OpCode(0x78, "CMP   d,#i        ", "cmp   $0{1},#${0}", 3, " 5 ", "(d) - i                          ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x5A, new OpCode(0x5A, "CMPW  YA,d        ", "cmpw  ya,$0{0}", 2, " 4 ", "YA - (d)                         ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0xDF, new OpCode(0xDF, "DAA   A           ", "daa   a", 1, " 3 ", "decimal adjust for addition      ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0xBE, new OpCode(0xBE, "DAS   A           ", "das   a", 1, " 3 ", "decimal adjust for subtraction   ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0xFE, new OpCode(0xFE, "DBNZ  Y,r         ", "dbnz  y,${2}", 2, "4/6", "Y-- then JNZ                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x6E, new OpCode(0x6E, "DBNZ  d,r         ", "dbnz  $0{0},${3}", 3, "5/7", "(d)-- then JNZ                   ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x9C, new OpCode(0x9C, "DEC   A           ", "dec   a", 1, " 2 ", "A--                              ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x1D, new OpCode(0x1D, "DEC   X           ", "dec   x", 1, " 2 ", "X--                              ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xDC, new OpCode(0xDC, "DEC   Y           ", "dec   y", 1, " 2 ", "Y--                              ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x8B, new OpCode(0x8B, "DEC   d           ", "dec   $0{0}", 2, " 4 ", "(d)--                            ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x9B, new OpCode(0x9B, "DEC   d+X         ", "dec   $0{0}+x", 2, " 5 ", "(d+X)--                          ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x8C, new OpCode(0x8C, "DEC   !a          ", "dec   ${1}{0}", 3, " 5 ", "(a)--                            ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x1A, new OpCode(0x1A, "DECW  d           ", "decw  $0{0}", 2, " 6 ", "Word (d)--                       ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xC0, new OpCode(0xC0, "DI                ", "di", 1, " 3 ", "I = 0                            ", ".", ".", ".", ".", ".", "0", ".", ".") },
            { 0x9E, new OpCode(0x9E, "DIV   YA,X        ", "div   ya,x", 1, "12 ", "A=YA/X, Y=mod(YA,X)              ", "N", "V", ".", ".", "H", ".", "Z", ".") },
            { 0xA0, new OpCode(0xA0, "EI                ", "ei", 1, " 3 ", "I = 1                            ", ".", ".", ".", ".", ".", "1", ".", ".") },
            { 0x59, new OpCode(0x59, "EOR   (X),(Y)     ", "eor   (x),(y)", 1, " 5 ", "(X) = (X) EOR (Y)                ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x48, new OpCode(0x48, "EOR   A,#i        ", "eor   a,#${0}", 2, " 2 ", "A = A EOR i                      ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x46, new OpCode(0x46, "EOR   A,(X)       ", "eor   a,(x)", 1, " 3 ", "A = A EOR (X)                    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x57, new OpCode(0x57, "EOR   A,[d]+Y     ", "eor   a,($0{0})+y", 2, " 6 ", "A = A EOR ([d]+Y)                ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x47, new OpCode(0x47, "EOR   A,[d+X]     ", "eor   a,($0{0}+x)", 2, " 6 ", "A = A EOR ([d+X])                ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x44, new OpCode(0x44, "EOR   A,d         ", "eor   a,$0{0}", 2, " 3 ", "A = A EOR (d)                    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x54, new OpCode(0x54, "EOR   A,d+X       ", "eor   a,$0{0}+x", 2, " 4 ", "A = A EOR (d+X)                  ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x45, new OpCode(0x45, "EOR   A,!a        ", "eor   a,${1}{0}", 3, " 4 ", "A = A EOR (a)                    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x55, new OpCode(0x55, "EOR   A,!a+X      ", "eor   a,${1}{0}+x", 3, " 5 ", "A = A EOR (a+X)                  ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x56, new OpCode(0x56, "EOR   A,!a+Y      ", "eor   a,${1}{0}+y", 3, " 5 ", "A = A EOR (a+Y)                  ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x49, new OpCode(0x49, "EOR   dd,ds       ", "eor   $0{1},$0{0}", 3, " 6 ", "(dd) = (dd) EOR (ds)             ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x58, new OpCode(0x58, "EOR   d,#i        ", "eor   $0{1},#${0}", 3, " 5 ", "(d) = (d) EOR i                  ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x8A, new OpCode(0x8A, "EOR1  C,m.b       ", "eor1  c,${1}{0}", 3, " 5 ", "C = C EOR (m.b)                  ", ".", ".", ".", ".", ".", ".", ".", "C") },
            { 0xBC, new OpCode(0xBC, "INC   A           ", "inc   a", 1, " 2 ", "A++                              ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x3D, new OpCode(0x3D, "INC   X           ", "inc   x", 1, " 2 ", "X++                              ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xFC, new OpCode(0xFC, "INC   Y           ", "inc   y", 1, " 2 ", "Y++                              ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xAB, new OpCode(0xAB, "INC   d           ", "inc   $0{0}", 2, " 4 ", "(d)++                            ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xBB, new OpCode(0xBB, "INC   d+X         ", "inc   $0{0}+x", 2, " 5 ", "(d+X)++                          ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xAC, new OpCode(0xAC, "INC   !a          ", "inc   ${1}{0}", 3, " 5 ", "(a)++                            ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x3A, new OpCode(0x3A, "INCW  d           ", "incw  $0{0}", 2, " 6 ", "Word (d)++                       ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x1F, new OpCode(0x1F, "JMP   [!a+X]      ", "jmp   (${1}{0}+x)", 3, " 6 ", "PC = [a+X]                       ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x5F, new OpCode(0x5F, "JMP   !a          ", "jmp   ${1}{0}", 3, " 3 ", "PC = a                           ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x5C, new OpCode(0x5C, "LSR   A           ", "lsr   a", 1, " 2 ", "Right shift A: 0->high, low->C   ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x4B, new OpCode(0x4B, "LSR   d           ", "lsr   $0{0}", 2, " 4 ", "Right shift (d) as above         ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x5B, new OpCode(0x5B, "LSR   d+X         ", "lsr   $0{0}+x", 2, " 5 ", "Right shift (d+X) as above       ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x4C, new OpCode(0x4C, "LSR   !a          ", "lsr   ${1}{0}", 3, " 5 ", "Right shift (a) as above         ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0xAF, new OpCode(0xAF, "MOV   (X)+,A      ", "mov   (x)+,a", 1, " 4 ", "(X++) = A      (no read)         ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xC6, new OpCode(0xC6, "MOV   (X),A       ", "mov   (x),a", 1, " 4 ", "(X) = A        (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xD7, new OpCode(0xD7, "MOV   [d]+Y,A     ", "mov   ($0{0})+y,a", 2, " 7 ", "([d]+Y) = A    (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xC7, new OpCode(0xC7, "MOV   [d+X],A     ", "mov   ($0{0}+x),a", 2, " 7 ", "([d+X]) = A    (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xE8, new OpCode(0xE8, "MOV   A,#i        ", "mov   a,#${0}", 2, " 2 ", "A = i                            ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xE6, new OpCode(0xE6, "MOV   A,(X)       ", "mov   a,(x)", 1, " 3 ", "A = (X)                          ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xBF, new OpCode(0xBF, "MOV   A,(X)+      ", "mov   a,(x)+", 1, " 4 ", "A = (X++)                        ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xF7, new OpCode(0xF7, "MOV   A,[d]+Y     ", "mov   a,($0{0})+y", 2, " 6 ", "A = ([d]+Y)                      ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xE7, new OpCode(0xE7, "MOV   A,[d+X]     ", "mov   a,($0{0}+x)", 2, " 6 ", "A = ([d+X])                      ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x7D, new OpCode(0x7D, "MOV   A,X         ", "mov   a,x", 1, " 2 ", "A = X                            ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xDD, new OpCode(0xDD, "MOV   A,Y         ", "mov   a,y", 1, " 2 ", "A = Y                            ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xE4, new OpCode(0xE4, "MOV   A,d         ", "mov   a,$0{0}", 2, " 3 ", "A = (d)                          ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xF4, new OpCode(0xF4, "MOV   A,d+X       ", "mov   a,$0{0}+x", 2, " 4 ", "A = (d+X)                        ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xE5, new OpCode(0xE5, "MOV   A,!a        ", "mov   a,${1}{0}", 3, " 4 ", "A = (a)                          ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xF5, new OpCode(0xF5, "MOV   A,!a+X      ", "mov   a,${1}{0}+x", 3, " 5 ", "A = (a+X)                        ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xF6, new OpCode(0xF6, "MOV   A,!a+Y      ", "mov   a,${1}{0}+y", 3, " 5 ", "A = (a+Y)                        ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xBD, new OpCode(0xBD, "MOV   SP,X        ", "mov   sp,x", 1, " 2 ", "SP = X                           ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xCD, new OpCode(0xCD, "MOV   X,#i        ", "mov   x,#${0}", 2, " 2 ", "X = i                            ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x5D, new OpCode(0x5D, "MOV   X,A         ", "mov   x,a", 1, " 2 ", "X = A                            ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x9D, new OpCode(0x9D, "MOV   X,SP        ", "mov   x,sp", 1, " 2 ", "X = SP                           ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xF8, new OpCode(0xF8, "MOV   X,d         ", "mov   x,$0{0}", 2, " 3 ", "X = (d)                          ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xF9, new OpCode(0xF9, "MOV   X,d+Y       ", "mov   x,$0{0}+y", 2, " 4 ", "X = (d+Y)                        ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xE9, new OpCode(0xE9, "MOV   X,!a        ", "mov   x,${1}{0}", 3, " 4 ", "X = (a)                          ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x8D, new OpCode(0x8D, "MOV   Y,#i        ", "mov   y,#${0}", 2, " 2 ", "Y = i                            ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xFD, new OpCode(0xFD, "MOV   Y,A         ", "mov   y,a", 1, " 2 ", "Y = A                            ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xEB, new OpCode(0xEB, "MOV   Y,d         ", "mov   y,$0{0}", 2, " 3 ", "Y = (d)                          ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xFB, new OpCode(0xFB, "MOV   Y,d+X       ", "mov   y,$0{0}+x", 2, " 4 ", "Y = (d+X)                        ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xEC, new OpCode(0xEC, "MOV   Y,!a        ", "mov   y,${1}{0}", 3, " 4 ", "Y = (a)                          ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xFA, new OpCode(0xFA, "MOV   dd,ds       ", "mov   $0{1},$0{0}", 3, " 5 ", "(dd) = (ds)    (no read)         ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xD4, new OpCode(0xD4, "MOV   d+X,A       ", "mov   $0{0}+x,a", 2, " 5 ", "(d+X) = A      (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xDB, new OpCode(0xDB, "MOV   d+X,Y       ", "mov   $0{0}+x,y", 2, " 5 ", "(d+X) = Y      (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xD9, new OpCode(0xD9, "MOV   d+Y,X       ", "mov   $0{0}+y,x", 2, " 5 ", "(d+Y) = X      (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x8F, new OpCode(0x8F, "MOV   d,#i        ", "mov   $0{1},#${0}", 3, " 5 ", "(d) = i        (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xC4, new OpCode(0xC4, "MOV   d,A         ", "mov   $0{0},a", 2, " 4 ", "(d) = A        (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xD8, new OpCode(0xD8, "MOV   d,X         ", "mov   $0{0},x", 2, " 4 ", "(d) = X        (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xCB, new OpCode(0xCB, "MOV   d,Y         ", "mov   $0{0},y", 2, " 4 ", "(d) = Y        (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xD5, new OpCode(0xD5, "MOV   !a+X,A      ", "mov   ${1}{0}+x,a", 3, " 6 ", "(a+X) = A      (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xD6, new OpCode(0xD6, "MOV   !a+Y,A      ", "mov   ${1}{0}+y,a", 3, " 6 ", "(a+Y) = A      (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xC5, new OpCode(0xC5, "MOV   !a,A        ", "mov   ${1}{0},a", 3, " 5 ", "(a) = A        (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xC9, new OpCode(0xC9, "MOV   !a,X        ", "mov   ${1}{0},x", 3, " 5 ", "(a) = X        (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xCC, new OpCode(0xCC, "MOV   !a,Y        ", "mov   ${1}{0},y", 3, " 5 ", "(a) = Y        (read)            ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xAA, new OpCode(0xAA, "MOV1  C,m.b       ", "mov1  c,${1}{0}", 3, " 4 ", "C = (m.b)                        ", ".", ".", ".", ".", ".", ".", ".", "C") },
            { 0xCA, new OpCode(0xCA, "MOV1  m.b,C       ", "mov1  ${1}{0},c", 3, " 6 ", "(m.b) = C                        ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xBA, new OpCode(0xBA, "MOVW  YA,d        ", "movw  ya,$0{0}", 2, " 5 ", "YA = word (d)                    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0xDA, new OpCode(0xDA, "MOVW  d,YA        ", "movw  $0{0},ya", 2, " 5 ", "word (d) = YA  (read low only)   ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xCF, new OpCode(0xCF, "MUL   YA          ", "mul   ya", 1, " 9 ", "YA = Y * A, NZ on Y only         ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x00, new OpCode(0x00, "NOP               ", "nop", 1, " 2 ", "do nothing                       ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xEA, new OpCode(0xEA, "NOT1  m.b         ", "not1  ${1}{0}", 3, " 5 ", "m.b = ~m.b                       ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xED, new OpCode(0xED, "NOTC              ", "notc", 1, " 3 ", "C = !C                           ", ".", ".", ".", ".", ".", ".", ".", "C") },
            { 0x19, new OpCode(0x19, "OR    (X),(Y)     ", "or    (x),(y)", 1, " 5 ", "(X) = (X) | (Y)                  ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x08, new OpCode(0x08, "OR    A,#i        ", "or    a,#${0}", 2, " 2 ", "A = A | i                        ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x06, new OpCode(0x06, "OR    A,(X)       ", "or    a,(x)", 1, " 3 ", "A = A | (X)                      ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x17, new OpCode(0x17, "OR    A,[d]+Y     ", "or    a,($0{0})+y", 2, " 6 ", "A = A | ([d]+Y)                  ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x07, new OpCode(0x07, "OR    A,[d+X]     ", "or    a,($0{0}+x)", 2, " 6 ", "A = A | ([d+X])                  ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x04, new OpCode(0x04, "OR    A,d         ", "or    a,$0{0}", 2, " 3 ", "A = A | (d)                      ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x14, new OpCode(0x14, "OR    A,d+X       ", "or    a,$0{0}+x", 2, " 4 ", "A = A | (d+X)                    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x05, new OpCode(0x05, "OR    A,!a        ", "or    a,${1}{0}", 3, " 4 ", "A = A | (a)                      ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x15, new OpCode(0x15, "OR    A,!a+X      ", "or    a,${1}{0}+x", 3, " 5 ", "A = A | (a+X)                    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x16, new OpCode(0x16, "OR    A,!a+Y      ", "or    a,${1}{0}+y", 3, " 5 ", "A = A | (a+Y)                    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x09, new OpCode(0x09, "OR    dd,ds       ", "or    $0{1},$0{0}", 3, " 6 ", "(dd) = (dd) | (ds)               ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x18, new OpCode(0x18, "OR    d,#i        ", "or    $0{1},#${0}", 3, " 5 ", "(d) = (d) | i                    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x2A, new OpCode(0x2A, "OR1   C,/m.b      ", "or1   c,${1}{0}", 3, " 5 ", "C = C | ~(m.b)                   ", ".", ".", ".", ".", ".", ".", ".", "C") },
            { 0x0A, new OpCode(0x0A, "OR1   C,m.b       ", "or1   ${1}{0},c", 3, " 5 ", "C = C | (m.b)                    ", ".", ".", ".", ".", ".", ".", ".", "C") },
            { 0x4F, new OpCode(0x4F, "PCALL u           ", "pcall $FF{0}", 2, " 6 ", "CALL $FF00+u                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xAE, new OpCode(0xAE, "POP   A           ", "pop   a", 1, " 4 ", "A = (++SP)                       ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x8E, new OpCode(0x8E, "POP   PSW         ", "pop   psw", 1, " 4 ", "Flags = (++SP)                   ", "N", "V", "P", "B", "H", "I", "Z", "C") },
            { 0xCE, new OpCode(0xCE, "POP   X           ", "pop   x", 1, " 4 ", "X = (++SP)                       ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xEE, new OpCode(0xEE, "POP   Y           ", "pop   y", 1, " 4 ", "Y = (++SP)                       ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x2D, new OpCode(0x2D, "PUSH  A           ", "push  a", 1, " 4 ", "(SP--) = A                       ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x0D, new OpCode(0x0D, "PUSH  PSW         ", "push  psw", 1, " 4 ", "(SP--) = Flags                   ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x4D, new OpCode(0x4D, "PUSH  X           ", "push  x", 1, " 4 ", "(SP--) = X                       ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x6D, new OpCode(0x6D, "PUSH  Y           ", "push  y", 1, " 4 ", "(SP--) = Y                       ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x6F, new OpCode(0x6F, "RET               ", "ret", 1, " 5 ", "Pop PC                           ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x7F, new OpCode(0x7F, "RET1              ", "ret1", 1, " 6 ", "Pop Flags, PC                    ", "N", "V", "P", "B", "H", "I", "Z", "C") },
            { 0x3C, new OpCode(0x3C, "ROL   A           ", "rol   a", 1, " 2 ", "Left shift A: low=C, C=high      ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x2B, new OpCode(0x2B, "ROL   d           ", "rol   $0{0}", 2, " 4 ", "Left shift (d) as above          ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x3B, new OpCode(0x3B, "ROL   d+X         ", "rol   $0{0}+x", 2, " 5 ", "Left shift (d+X) as above        ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x2C, new OpCode(0x2C, "ROL   !a          ", "rol   ${1}{0}", 3, " 5 ", "Left shift (a) as above          ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x7C, new OpCode(0x7C, "ROR   A           ", "ror   a", 1, " 2 ", "Right shift A: high=C, C=low     ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x6B, new OpCode(0x6B, "ROR   d           ", "ror   $0{0}", 2, " 4 ", "Right shift (d) as above         ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x7B, new OpCode(0x7B, "ROR   d+X         ", "ror   $0{0}+x", 2, " 5 ", "Right shift (d+X) as above       ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0x6C, new OpCode(0x6C, "ROR   !a          ", "ror   ${1}{0}", 3, " 5 ", "Right shift (a) as above         ", "N", ".", ".", ".", ".", ".", "Z", "C") },
            { 0xB9, new OpCode(0xB9, "SBC   (X),(Y)     ", "sbc   (x),(y)", 1, " 5 ", "(X) = (X)-(Y)-!C                 ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0xA8, new OpCode(0xA8, "SBC   A,#i        ", "sbc   a,#${0}", 2, " 2 ", "A = A-i-!C                       ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0xA6, new OpCode(0xA6, "SBC   A,(X)       ", "sbc   a,(x)", 1, " 3 ", "A = A-(X)-!C                     ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0xB7, new OpCode(0xB7, "SBC   A,[d]+Y     ", "sbc   a,($0{0})+y", 2, " 6 ", "A = A-([d]+Y)-!C                 ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0xA7, new OpCode(0xA7, "SBC   A,[d+X]     ", "sbc   a,($0{0}+x)", 2, " 6 ", "A = A-([d+X])-!C                 ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0xA4, new OpCode(0xA4, "SBC   A,d         ", "sbc   a,$0{0}", 2, " 3 ", "A = A-(d)-!C                     ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0xB4, new OpCode(0xB4, "SBC   A,d+X       ", "sbc   a,$0{0}+x", 2, " 4 ", "A = A-(d+X)-!C                   ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0xA5, new OpCode(0xA5, "SBC   A,!a        ", "sbc   a,${1}{0}", 3, " 4 ", "A = A-(a)-!C                     ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0xB5, new OpCode(0xB5, "SBC   A,!a+X      ", "sbc   a,${1}{0}+x", 3, " 5 ", "A = A-(a+X)-!C                   ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0xB6, new OpCode(0xB6, "SBC   A,!a+Y      ", "sbc   a,${1}{0}+y", 3, " 5 ", "A = A-(a+Y)-!C                   ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0xA9, new OpCode(0xA9, "SBC   dd,ds       ", "sbc   $0{1},$0{0}", 3, " 6 ", "(dd) = (dd)-(ds)-!C              ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0xB8, new OpCode(0xB8, "SBC   d,#i        ", "sbc   $0{1},#${0}", 3, " 5 ", "(d) = (d)-i-!C                   ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x02, new OpCode(0x02, "SET1  d.0         ", "set0  $0{0}", 2, " 4 ", "d.0 = 1                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x22, new OpCode(0x22, "SET1  d.1         ", "set1  $0{0}", 2, " 4 ", "d.1 = 1                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x42, new OpCode(0x42, "SET1  d.2         ", "set2  $0{0}", 2, " 4 ", "d.2 = 1                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x62, new OpCode(0x62, "SET1  d.3         ", "set3  $0{0}", 2, " 4 ", "d.3 = 1                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x82, new OpCode(0x82, "SET1  d.4         ", "set4  $0{0}", 2, " 4 ", "d.4 = 1                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xA2, new OpCode(0xA2, "SET1  d.5         ", "set5  $0{0}", 2, " 4 ", "d.5 = 1                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xC2, new OpCode(0xC2, "SET1  d.6         ", "set6  $0{0}", 2, " 4 ", "d.6 = 1                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xE2, new OpCode(0xE2, "SET1  d.7         ", "set7  $0{0}", 2, " 4 ", "d.7 = 1                          ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x80, new OpCode(0x80, "SETC              ", "setc", 1, " 2 ", "C = 1                            ", ".", ".", ".", ".", ".", ".", ".", "1") },
            { 0x40, new OpCode(0x40, "SETP              ", "setp", 1, " 2 ", "P = 1                            ", ".", ".", "1", ".", ".", ".", ".", ".") },
            { 0xEF, new OpCode(0xEF, "SLEEP             ", "sleep", 1, " ? ", "Halts the processor              ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xFF, new OpCode(0xFF, "STOP              ", "stop", 1, " ? ", "Halts the processor              ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x9A, new OpCode(0x9A, "SUBW  YA,d        ", "subw  ya,$0{0}", 2, " 5 ", "YA  = YA - (d), H on high byte   ", "N", "V", ".", ".", "H", ".", "Z", "C") },
            { 0x01, new OpCode(0x01, "TCALL 0           ", "tcall 0", 1, " 8 ", "CALL [$FFDE]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x11, new OpCode(0x11, "TCALL 1           ", "tcall 1", 1, " 8 ", "CALL [$FFDC]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x21, new OpCode(0x21, "TCALL 2           ", "tcall 2", 1, " 8 ", "CALL [$FFDA]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x31, new OpCode(0x31, "TCALL 3           ", "tcall 3", 1, " 8 ", "CALL [$FFD8]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x41, new OpCode(0x41, "TCALL 4           ", "tcall 4", 1, " 8 ", "CALL [$FFD6]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x51, new OpCode(0x51, "TCALL 5           ", "tcall 5", 1, " 8 ", "CALL [$FFD4]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x61, new OpCode(0x61, "TCALL 6           ", "tcall 6", 1, " 8 ", "CALL [$FFD2]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x71, new OpCode(0x71, "TCALL 7           ", "tcall 7", 1, " 8 ", "CALL [$FFD0]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x81, new OpCode(0x81, "TCALL 8           ", "tcall 8", 1, " 8 ", "CALL [$FFCE]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x91, new OpCode(0x91, "TCALL 9           ", "tcall 9", 1, " 8 ", "CALL [$FFCC]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xA1, new OpCode(0xA1, "TCALL 10          ", "tcall 10", 1, " 8 ", "CALL [$FFCA]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xB1, new OpCode(0xB1, "TCALL 11          ", "tcall 11", 1, " 8 ", "CALL [$FFC8]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xC1, new OpCode(0xC1, "TCALL 12          ", "tcall 12", 1, " 8 ", "CALL [$FFC6]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xD1, new OpCode(0xD1, "TCALL 13          ", "tcall 13", 1, " 8 ", "CALL [$FFC4]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xE1, new OpCode(0xE1, "TCALL 14          ", "tcall 14", 1, " 8 ", "CALL [$FFC2]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0xF1, new OpCode(0xF1, "TCALL 15          ", "tcall 15", 1, " 8 ", "CALL [$FFC0]                     ", ".", ".", ".", ".", ".", ".", ".", ".") },
            { 0x4E, new OpCode(0x4E, "TCLR1 !a          ", "tclr  ${1}{0},a", 3, " 6 ", "(a) = (a)&~A, ZN as for A-(a)    ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x0E, new OpCode(0x0E, "TSET1 !a          ", "tset  ${1}{0},a", 3, " 6 ", "(a) = (a)|A, ZN as for A-(a)     ", "N", ".", ".", ".", ".", ".", "Z", ".") },
            { 0x9F, new OpCode(0x9F, "XCN   A           ", "xcn   a", 1, " 5 ", "A = (A>>4) | (A<<4)              ", "N", ".", ".", ".", ".", ".", "Z", ".") },
        };
    }

    public class OpCode
    {
        public int opcode { get; set; }
        public string instruction { get; set; }
        public string formattedinstruction { get; set; }
        public int bytelength { get; set; }
        public string cycles { get; set; }
        public string operation { get; set; }
        public string N { get; set; }
        public string V { get; set; }
        public string P { get; set; }
        public string B { get; set; }
        public string H { get; set; }
        public string I { get; set; }
        public string Z { get; set; }
        public string C { get; set; }

        public string GetInstructionWithValues(int a, int x, int y, int pc, int dp, int sp, byte[] memory)
        {
            byte lowByte = memory[pc];
            byte highByte = memory[pc + 1];
            int branchPC = pc + 1 + (sbyte)lowByte;
            int branchPC2 = pc + 2 + (sbyte)highByte;
            //int dpRelativeAddress = dp + lowByte;
            //byte dpRelative = memory[dpRelativeAddress];

            var output = String.Format(formattedinstruction, lowByte.ToString("x2"), highByte.ToString("x2"), branchPC.ToString("x4"), branchPC2.ToString("x4"))
                .PadRight(22, ' ')
                .ToLower();

            return output;
        }
        public OpCode(
                int opcode,
                string instruction,
                string formattedinstruction,
                int bytelength,
                string cycles,
                string operation,
                string N,
                string V,
                string P,
                string B,
                string H,
                string I,
                string Z,
                string C
         )
        {
            this.opcode = opcode;
            this.instruction = instruction;
            this.formattedinstruction = formattedinstruction;
            this.bytelength = bytelength;
            this.cycles = cycles;
            this.operation = operation;
            this.N = N;
            this.V = V;
            this.P = P;
            this.B = B;
            this.H = H;
            this.I = I;
            this.Z = Z;
            this.C = C;
        }
    }
}

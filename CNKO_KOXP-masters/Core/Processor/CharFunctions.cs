using static KOXP.Constants.Addresses.AddressExtensions;
using static KOXP.Constants.Addresses.AddressHandler;
using static KOXP.Core.Processor.Functions;
using KOXP.Constants;

namespace KOXP.Core.Processor
{
    public class CharFunctions : Address
    {
        public static bool IsThereAPartyReq()
        {
            return Read4Byte(Read4Byte(KO_PTR_MSGBOX) + KO_OFF_MSGBOX) != 0;
        }

        public static int GetClass()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_CLASS);
        }

        public static int GetWeight()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_WEIGHT);
        }

        public static int GetMaxWeight()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_MAX_WEIGHT);
        }

        public static int GetTargetId()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_MOB);
        }

        public static string TargetName()
        {
            if (GetTargetId() == 0)
                return "";

            int Base = GetTargetBase();

            if (Base == 0)
                return "";

            int NameLen = Read4Byte(Base + KO_OFF_NAME_LEN);
            return NameLen > 15 ? ReadString(Read4Byte(Base + KO_OFF_NAME), NameLen) : ReadString(Base + KO_OFF_NAME, NameLen);
        }

        public static int GetZoneId()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_ZONE);
        }

        public static float GetExp()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_EXP);
        }

        public static float GetMaxExp()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_MAX_EXP);
        }

        public static int TargetX()
        {
            if (GetTargetId() == 0)
                return 0;

            int Base = GetTargetBase();

            return Base == 0 ? 0 : (int)Math.Round(ReadFloat(Base + KO_OFF_X));
        }

        public static int TargetY()
        {
            if (GetTargetId() == 0)
            {
                return 0;
            }

            int Base = GetTargetBase();
            return Base == 0 ? 0 : (int)Math.Round(ReadFloat(Base + KO_OFF_Y));
        }

        public static int GetTargetX(int targetBase)
        {
            return (int)Math.Round(ReadFloat(targetBase + KO_OFF_X));
        }

        public static int GetTargetY(int targetBase)
        {
            return (int)Math.Round(ReadFloat(targetBase + KO_OFF_Y));
        }

        public static int GetTargetZ(int targetBase)
        {
            return (int)Math.Round(ReadFloat(targetBase + KO_OFF_Z));
        }

        public static int GetTargetId(int targetBase)
        {
            return Read4Byte(targetBase + KO_OFF_ID);
        }

        public static int GetTargetHealth(int targetBase)
        {
            return Read4Byte(targetBase + KO_OFF_HP);
        }

        public static int GetTargetMaxHealth(int targetBase)
        {
            return Read4Byte(targetBase + KO_OFF_MAX_HP);
        }

        public static int GetTargetRaceType(int targetBase)
        {
            return Read4Byte(targetBase + KO_OFF_NATION);
        }

        public static int GetTargetMoveType(int targetBase)
        {
            return ReadByte(targetBase + KO_OFF_TARGET_MOVE);
        }

        public static int GetTargetState(int targetBase)
        {
            return ReadByte(targetBase + KO_OFF_STATE);
        }

        public static string GetTargetName(int targetBase)
        {
            int nameLen = Read4Byte(targetBase + KO_OFF_NAME_LEN);
            return nameLen > 15 ? ReadString(Read4Byte(targetBase + KO_OFF_NAME), nameLen) : ReadString(targetBase + KO_OFF_NAME, nameLen);
        }

        public static void Wallhack(bool Enable)
        {
            Write4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_WH, Enable ? 0 : 1);
        }

        public static bool DC()
        {
            return Read4Byte(Read4Byte(KO_PTR_PKT) + KO_OFF_DC) == 0;
        }

        public static int GetPartyBase(int Member)
        {
            int Base = Read4Byte(Read4Byte(Read4Byte(Read4Byte(KO_PTR_DLG) + KO_OFF_PTBASE) + KO_OFF_PT)); // for member id KO_OFF_PT = 0x2E4

            switch (Member)
            {
                case 0: return Base;
                case 1: return Read4Byte(Base + 0x0);
                case 2: return Read4Byte(Read4Byte(Base + 0x0) + 0x0);
                case 3: return Read4Byte(Read4Byte(Read4Byte(Base + 0x0) + 0x0) + 0x0);
                case 4: return Read4Byte(Read4Byte(Read4Byte(Read4Byte(Base + 0x0) + 0x0) + 0x0) + 0x0);
                case 5: return Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(Base + 0x0) + 0x0) + 0x0) + 0x0) + 0x0);
                case 6: return Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(Base + 0x0) + 0x0) + 0x0) + 0x0) + 0x0) + 0x0);
                case 7: return Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(Base + 0x0) + 0x0) + 0x0) + 0x0) + 0x0) + 0x0) + 0x0);
                case 8: return Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(Base + 0x0) + 0x0) + 0x0) + 0x0) + 0x0) + 0x0) + 0x0) + 0x0);
                default: return 0;
            }
        }

        public static string GetPartyName(int Member)
        {
            int MemberNickLen = Read4Byte(GetPartyBase(Member) + 0x44);

            return ReadString(GetPartyBase(Member) + 0x34, MemberNickLen);
        }

        public static int GetPartyCount()
        {
            return Read4Byte(Read4Byte(Read4Byte(KO_PTR_DLG) + KO_OFF_PTBASE) + KO_OFF_PTCOUNT);
        }

        public static string GetName()
        {
            int NameLen = Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_NAME_LEN);

            return NameLen > 15
                ? ReadString(Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_NAME), NameLen)
                : ReadString(Read4Byte(KO_PTR_CHR) + KO_OFF_NAME, NameLen);
        }

        public static int GetPetId()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_PET_ID);
        }

        public static int GetId()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_ID);
        }

        public static int GetHp()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_HP);
        }
        public static int GetMaxHp()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_MAX_HP);
        }

        public static int GetMp()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_MP);
        }

        public static int GetMaxMp()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_MAX_MP);
        }

        public static int GetX()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(KO_PTR_CHR) + KO_OFF_X));
        }

        public static int GetY()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(KO_PTR_CHR) + KO_OFF_Y));
        }

        public static string FGetX()
        {
            float x = ReadFloat(Read4Byte(KO_PTR_CHR) + KO_OFF_X);
            var strArr = x.ToString().Split('.').ToArray();
            string finalString = strArr[0].PadLeft(3, '0') + strArr[1].Substring(0, 1);

            return finalString;
        }

        public static string FGetY()
        {
            float x = ReadFloat(Read4Byte(KO_PTR_CHR) + KO_OFF_Y);
            var strArr = x.ToString().Split('.').ToArray();
            string finalString = strArr[0].PadLeft(3, '0') + strArr[1].Substring(0, 1);

            return finalString;
        }

        public static int TruncateFloatGetX()
        {
            float X = ReadFloat(Read4Byte(KO_PTR_CHR) + KO_OFF_X);
            int truncateFloat = (int)X * 10;

            return truncateFloat;
        }

        public static int TruncateFloatGetY()
        {
            float Y = ReadFloat(Read4Byte(KO_PTR_CHR) + KO_OFF_Y);
            int truncateFloat = (int)Y * 10;

            return truncateFloat;
        }

        public static int TruncateFloatGetZ()
        {
            float Z = ReadFloat(Read4Byte(KO_PTR_CHR) + KO_OFF_Z);
            int truncateFloat = (int)Z * 10;

            return truncateFloat;
        }

        public static string FGetZ()
        {
            float x = ReadFloat(Read4Byte(KO_PTR_CHR) + KO_OFF_Z);
            var strArr = x.ToString().Split('.').ToArray();
            string finalString = strArr[0].PadLeft(3, '0') + strArr[1].Substring(0, 1);

            return finalString;
        }

        public static int GetZ()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(KO_PTR_CHR) + KO_OFF_Z));
        }

        public static string GetJob()
        {
            int ClassId = GetClass();

            string Job = "Undefined";

            switch (ClassId)
            {
                case 102: Job = "Rogue"; break;
                case 107: Job = "Rogue"; break;
                case 108: Job = "Rogue"; break;
                case 202: Job = "Rogue"; break;
                case 207: Job = "Rogue"; break;
                case 208: Job = "Rogue"; break;
                case 104: Job = "Priest"; break;
                case 111: Job = "Priest"; break;
                case 112: Job = "Priest"; break;
                case 204: Job = "Priest"; break;
                case 211: Job = "Priest"; break;
                case 212: Job = "Priest"; break;
                case 101: Job = "Warrior"; break;
                case 105: Job = "Warrior"; break;
                case 106: Job = "Warrior"; break;
                case 201: Job = "Warrior"; break;
                case 205: Job = "Warrior"; break;
                case 206: Job = "Warrior"; break;
                case 103: Job = "Mage"; break;
                case 109: Job = "Mage"; break;
                case 110: Job = "Mage"; break;
                case 203: Job = "Mage"; break;
                case 209: Job = "Mage"; break;
                case 210: Job = "Mage"; break;
            }

            return Job;
        }

        public static byte GetPetHp()
        {
            return (byte)Math.Round(ReadFloat(Read4Byte(Read4Byte(Read4Byte(KO_PTR_DLG) + 0x3AC) + 0x114) + 0x134));
        }

        public static byte GetPetMp()
        {
            return (byte)ReadFloat(Read4Byte(Read4Byte(Read4Byte(KO_PTR_DLG) + 0x3AC) + 0x110) + 0x134);
        }

        public static byte GetPetHunger()
        {
            return (byte)ReadFloat(Read4Byte(Read4Byte(Read4Byte(KO_PTR_DLG) + 0x3AC) + 0x120) + 0x12C);
        }

        public static byte GetPetExp()
        {
            return (byte)ReadFloat(Read4Byte(Read4Byte(Read4Byte(KO_PTR_DLG) + 0x3AC) + 0x124) + 0x134);
        }
    }
}

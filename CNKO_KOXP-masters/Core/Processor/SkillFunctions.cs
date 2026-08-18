using KOXP.Constants.Addresses;
using static KOXP.Constants.Address;
using static KOXP.Core.Processor.CharFunctions;

namespace KOXP.Core.Processor
{
    public class SkillFunctions : AddressExtensions
    {
        public static bool IsSkillInUsed(int SkillID)
        {
            for (int i = 0; i <= GetSkillCount(); i++)
            {
                if (GetCurrentSkill(i) == SkillID)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool SkillOverlap(string SkillName)
        {
            switch (SkillName)
            {
                case "Evade":
                    {
                        if (IsSkillInUsed(int.Parse(GetClass().ToString() + "730")))
                            return true;

                        if (IsSkillInUsed(int.Parse(GetClass().ToString() + "760")))
                            return true;

                        if (IsSkillInUsed(500343))
                            return true;
                        break;
                    }
                case "Safely":
                    {
                        if (IsSkillInUsed(int.Parse(GetClass().ToString() + "710")))
                            return true;

                        if (IsSkillInUsed(int.Parse(GetClass().ToString() + "760")))
                            return true;

                        if (IsSkillInUsed(500343))
                            return true;
                        break;
                    }
                case "Scaled Skin":
                    {
                        if (IsSkillInUsed(int.Parse(GetClass().ToString() + "710")))
                            return true;

                        if (IsSkillInUsed(int.Parse(GetClass().ToString() + "730")))
                            return true;

                        if (IsSkillInUsed(500343))
                            return true;
                        break;
                    }
                case "Light Feet":
                    {
                        if (IsSkillInUsed(int.Parse(GetClass().ToString() + "010")))
                            return true;
                        break;
                    }
                case "Swift":
                    {
                        if (IsSkillInUsed(int.Parse(GetClass().ToString() + "725")))
                            return true;
                        break;
                    }
                case "Wolf":
                    {
                        if (IsSkillInUsed(500342))
                            return true;

                        if (IsSkillInUsed(501139))
                            return true;
                        break;
                    }
            }
            return false;
        }

        public static int GetSkillCount()
        {
            int Ptr, tmpBase;

            Ptr = Read4Byte(KO_PTR_DLG);

            tmpBase = Read4Byte(Ptr + KO_OFF_USE_SKILL_BASE);
            tmpBase = Read4Byte(tmpBase + 0x4);
            tmpBase = Read4Byte(tmpBase + KO_OFF_USE_SKILL_ID + 0x4);
            return tmpBase;
        }

        public static int GetCurrentSkill(int SkillNo)
        {
            int Ptr, tmpBase;
            Ptr = Read4Byte(KO_PTR_DLG);
            tmpBase = Read4Byte(Ptr + KO_OFF_USE_SKILL_BASE);
            tmpBase = Read4Byte(tmpBase + 0x4);
            tmpBase = Read4Byte(tmpBase + KO_OFF_USE_SKILL_ID);

            for (int i = 0; i <= SkillNo; i++)
            {
                tmpBase = Read4Byte(tmpBase + 0x0);
            }
            tmpBase = Read4Byte(tmpBase + 0x8);
            if (tmpBase > 0)
            {
                tmpBase = Read4Byte(tmpBase + 0x0);
                return tmpBase;
            }
            else
            {
                return 0;
            }
        }
    }
}

namespace KOXP.Constants
{
    public class Address
    {
        //Pointers
        public static int KO_PTR_MSGBOX = 0xF7E340;

        public static int KO_PTR_CHR = 0xF7E2F4;

        public static int KO_PTR_DLG = 0xF7E354;
        public static int KO_PTR_PKT = 0xF7E32C;
        public static int KO_PTR_SND = 0x5ED620;

        public static int KO_PTR_FNCZ = 0x71A413;
        public static int KO_PTR_FLDB = KO_PTR_CHR - 4;
        public static int KO_PTR_FMBS = 0x654A10;

        public static int KO_OFF_06 = 0x712370;

        public static int KO_PERI_PTR = 0x7C10F0;

        //Offsets
        public static int KO_OFF_MSGBOX = 0x28;

        public static int KO_OFF_ID = 0x648;
        public static int KO_OFF_NAME = 0x64C;
        public static int KO_OFF_NAME_LEN = 0x65C;

        public static int KO_OFF_HP = 0x680;
        public static int KO_OFF_MAX_HP = 0x67C;
        public static int KO_OFF_MP = 0xB28;
        public static int KO_OFF_MAX_MP = 0xB24;

        public static int KO_OFF_CLASS = 0x674;

        public static int KO_OFF_EXP = 0xB48;
        public static int KO_OFF_MAX_EXP = 0xB40;
        public static int KO_OFF_LVL = 0x678;

        public static int KO_OFF_GOLD = 0xB38;
        public static int KO_OFF_WEIGHT = 0xB60;
        public static int KO_OFF_MAX_WEIGHT = 0xB58;

        public static int KO_OFF_ATTACK = 0xB9C;
        public static int KO_OFF_DEFENCE = 0xBA4;

        public static int KO_OFF_STATP = 0xB1C;
        public static int KO_OFF_STRP = 0xB64;
        public static int KO_OFF_HPP = 0xB6C;
        public static int KO_OFF_DEXP = 0xB74;
        public static int KO_OFF_MPP = 0xB84;
        public static int KO_OFF_INTP = 0xB7C;

        public static int KO_OFF_MOVETYPE = 0x3B8;
        public static int KO_OFF_MOVE = 0xF30;

        public static int KO_OFF_MOUSE_X = 0xF3C;
        public static int KO_OFF_MOUSE_Y = 0xF44;
        public static int KO_OFF_MOUSE_Z = 0xF40;

        public static int KO_OFF_X = 0xD0;
        public static int KO_OFF_Y = 0xD8;
        public static int KO_OFF_Z = 0xD4;

        public static int KO_OFF_TARGET_MOVE = 0x3AC;
        public static int KO_OFF_TARGET_STATU = 0x270;
        public static int KO_OFF_STATE = 0x2EC;

        public static int KO_OFF_NATION = 0x66C;
        public static int KO_OFF_WH = 0x684;
        public static int KO_OFF_MOB = 0x604;
        public static int KO_OFF_ZONE = 0xBD0;

        public static int KO_OFF_LOOT = 0x948;

        public static int KO_OFF_SKILL_BASE = 0x1DC;
        public static int KO_OFF_USE_SKILL_BASE = 0x1C0;
        public static int KO_OFF_USE_SKILL_ID = 0x110;

        public static int KO_OFF_PT = 0x2F0;
        public static int KO_OFF_PTBASE = 0x1D8;
        public static int KO_OFF_PTCOUNT = 0x2E8;
        public static int KO_OFF_PTHP = 0x18;

        public static int KO_OFF_DC = 0xA0;

        public static int KO_OFF_PET_ID = 0xF74;

        public static int KO_OTO_BTN_PTR = KO_PTR_CHR + 0x5C;

        public static int KO_BTN_SKIP = 0x6B7370;
        public static int KO_BTN_LEFT = 0x6B6840;
        public static int KO_BTN_RIGHT = 0x6B68F0;
        public static int KO_BTN_LOGIN = 0x6AE530;
    }
}

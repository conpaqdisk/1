using KOXP.Common;
using KOXP.Constants;
using System.Diagnostics;
using static KOXP.Constants.Addresses.AddressExtensions;
using static KOXP.Constants.Addresses.AddressHandler;
using static KOXP.Constants.Handle;
using static KOXP.Core.Helper;
using static KOXP.Core.Processor.CharFunctions;
using static KOXP.Core.Processor.InventoryFunctions;
using static KOXP.Form1;
using static KOXP.Win32.Win32Api;
using static KOXP.Win32.Win32Enum;

namespace KOXP.Core.Processor
{
    public class Functions : Address
    {
        public static List<Thread> _ThreadPool { get; set; } = new List<Thread>();
        public static EAction _Action { get; set; }

        public static void StartThread(Action p)
        {
            Thread t = new(new ThreadStart(p))
            {
                IsBackground = true
            };
            t.Start();

            _ThreadPool.Add(t);
        }

        public enum EAction : byte
        {
            None,
            Attack,
            Supplying,
        }

        public static EAction GetAction()
        {
            return _Action;
        }

        public static void SetAction(EAction s)
        {
            _Action = s;
        }

        public static void SendParty(string? Name)
        {
            if (Name == null) { return; }
            SendPacket("2F01" + AlignDWORD(Name.Length).Substring(0, 2) + "00" + StringToHex(Name));
            Thread.Sleep(10);
            SendPacket("2F03" + AlignDWORD(Name.Length).Substring(0, 2) + "00" + StringToHex(Name));
        }

        public static bool IsPartyMember(string? PlayerName)
        {
            for (int i = 0; i <= GetPartyCount() - 1; i++)
            {
                if (GetPartyName(i) == PlayerName)
                    return true;
            }

            return false;
        }

        public static void GetOread()
        {
            ExecuteRemoteCode("608B0D" + AlignDWORD(KO_PTR_CHR) +
                              "6A0168" + AlignDWORD(700039000) + // Oreads id
                              "B8" + AlignDWORD(KO_PERI_PTR) +
                              "FFD0" +
                              "61" +
                              "C3");

            Write4Byte(Read4Byte(KO_PTR_DLG) + KO_OFF_LOOT, 1);
        }

        public static void GetMonsterStones()
        {
            for (int i = 2; i < 10; i++)
                SendPacket($"99028{i}01"); //Reach Level 10 ~ 80

            SendPacket("99025600"); //Merchant's Daughter
            SendPacket("99025800"); //Chief Guard Patrick
            SendPacket("99026500"); //Chief Hunting I
            SendPacket("99027400"); //Chief Hunting II
            SendPacket("99020200"); //10 Kill Görevi 1
            SendPacket("99021000"); //10 Kill Görevi 2
            SendPacket("99025F00"); //1.000 Mob kesme
            SendPacket("99026200"); //30.000 Mob kesme
            SendPacket("99026A00"); //Battle With Fog
        }

        public static bool IsSelectableTargetWithBase(int TargetBase)
        {
            if (GetHp() == 0)
                return false;

            if (TargetBase == 0)
                return false;

            //if (GetTargetMaxHealth(TargetBase) != 0 && GetTargetHealth(TargetBase) == 0)
            //    return false;

            return Distance(GetX(), GetY(), GetTargetX(TargetBase), GetTargetY(TargetBase)) <= targetSelectRange
                    && Read4Byte(TargetBase + KO_OFF_NATION) == 0 && GetTargetMoveType(TargetBase) != 4 && GetTargetState(TargetBase) != 11;
        }

        public static int GetMobBase(int MobId)
        {
            IntPtr Addr = VirtualAllocEx(GameProcessHandle, IntPtr.Zero, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

            ExecuteRemoteCode(
                "60" +
                "8B" +
                "0D" + AlignDWORD(KO_PTR_FLDB) +
                "6A01" + "68" + AlignDWORD(MobId) +
                "BF" + AlignDWORD(KO_PTR_FMBS) +
                "FFD7" + "A3" + AlignDWORD(Addr) +
                "61" + 
                "C3");

            int Base = Read4Byte(Addr);
            VirtualFreeEx(GameProcessHandle, Addr, 0, MEM_RELEASE);

            return Base;
        }

        public static int GetTargetBase(int TargetId = 0)
        {
            TargetId = TargetId > 0 ? TargetId : GetTargetId();

            return TargetId > 0 ? TargetId > 9999 ? GetMobBase(TargetId) : GetPlayerBase(TargetId) : 0;
        }

        public static int GetPlayerBase(int PlayerId)
        {
            IntPtr Addr = VirtualAllocEx(GameProcessHandle, IntPtr.Zero, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

            ExecuteRemoteCode("60" +
               "6A00" +
               "68" + AlignDWORD(PlayerId) +
               "8B0D" + AlignDWORD(KO_PTR_FLDB) +
               "B8" + AlignDWORD(KO_PTR_FMBS) +
               "FF" + "D0" +
               "A3" + AlignDWORD(Addr) + "61C3");
            int Base = Read4Byte(Addr);
            _ = VirtualFreeEx(GameProcessHandle, Addr, 0, MEM_RELEASE);
            return Base;
        }

        public static int Distance(int StartX, int StartY, int TargetX, int TargetY)
        {
            return Convert.ToInt32(Math.Sqrt(Math.Pow(TargetX - StartX, 2) + Math.Pow(TargetY - StartY, 2)));
        }

        public static bool GoToTarget(int GoX, int GoY)
        {
            if (Distance(GetX(), GetY(), GoX, GoY) == 0)
                return true;

            Write4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_MOVETYPE, 2);
            WriteFloat(Read4Byte(KO_PTR_CHR) + KO_OFF_MOUSE_X, GoX);
            WriteFloat(Read4Byte(KO_PTR_CHR) + KO_OFF_MOUSE_Y, GoY);
            Write4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_MOVE, 1);

            if (GetAction() != EAction.Attack)
                Thread.Sleep(10);

            return false;
        }

        public static bool IsMoving()
        {
            return Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_MOVE) == 1 || Read4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_MOVETYPE) == 2;
        }

        public static void UseSkillWhileWalking()
        {
            if (IsMoving())
                ExecuteRemoteCode("606A006A00B9" + AlignDWORD(KO_PTR_DLG) + "8B09B8" + AlignDWORD(KO_OFF_06) + "FFD061C3");
            //SendPacket("06" + AlignDWORD(TruncateFloatGetX())[..4] + AlignDWORD(TruncateFloatGetY())[..4] + AlignDWORD(TruncateFloatGetZ())[..4] + "000000" + AlignDWORD(TruncateFloatGetX())[..4] + AlignDWORD(TruncateFloatGetY())[..4] + AlignDWORD(TruncateFloatGetZ())[..4]);
        }

        public static bool IsAttackableTargetWithBase(int targetBase)
        {
            if (targetBase == 0)
                return false;

            return Distance(GetX(), GetY(), GetTargetX(targetBase), GetTargetY(targetBase)) <= attackRange
                    && GetTargetMoveType(targetBase) != 4 && GetTargetHealth(targetBase) != 0 && Read4Byte(targetBase + KO_OFF_NATION) == 0;
        }

        public static string FindHPPotion()
        {
            string strHPotion = "";

            int[] hPot = { 389015000, 389014000, 389013000, 389012000, 389011000, 389010000 };

            for (int i = 0; i < hPot.Length; i++)
            {
                if (GetItemsInInventory(hPot[i]))
                {
                    switch (hPot[i])
                    {
                        case 389015000: strHPotion = "015"; break;
                        case 389014000: strHPotion = "014"; break;
                        case 389013000: strHPotion = "013"; break;
                        case 389012000: strHPotion = "012"; break;
                        case 389011000: strHPotion = "011"; break;
                        case 389010000: strHPotion = "010"; break;
                    }
                }
            }
            return strHPotion;
        }

        public static string FindMPPotion()
        {
            string strMPotion = "";

            int[] mPot = { 389020000, 389019000, 389018000, 389017000, 389016000 };

            for (int i = 0; i < mPot.Length; i++)
            {
                if (GetItemsInInventory(mPot[i]))
                {
                    switch (mPot[i])
                    {
                        case 389020000: strMPotion = "020"; break;
                        case 389019000: strMPotion = "019"; break;
                        case 389018000: strMPotion = "018"; break;
                        case 389017000: strMPotion = "017"; break;
                        case 389016000: strMPotion = "016"; break;
                    }
                }
            }

            return strMPotion;
        }

        public static void PotionHP(string potion, bool pet)
        {
            string PotionID;
            switch (potion)
            {
                case "Automatic": PotionID = FindHPPotion(); break;
                case "720": PotionID = "014"; break;
                case "360": PotionID = "013"; break;
                case "180": PotionID = "012"; break;
                case "90": PotionID = "011"; break;
                case "45": PotionID = "010"; break;
                case "Ibexs": PotionID = "071"; break;
                default: return;
            }

            if (FindHPPotion() != "" && !pet)
            {
                SendPacket("3103" + AlignDWORD(int.Parse("490" + PotionID))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000000");
            }
            else
                SendPacket("760203" + AlignDWORD(int.Parse("490" + PotionID))[..8] + AlignDWORD(GetPetId())[..8] + AlignDWORD(GetPetId())[..8] + "000000000000000000000000000000000000000000000000");
        }

        public static void PotionMP(string potion, bool pet)
        {
            string PotionID;
            switch (potion)
            {
                case "Automatic": PotionID = FindMPPotion(); break;
                case "1920": PotionID = "020"; break;
                case "960": PotionID = "019"; break;
                case "480": PotionID = "018"; break;
                case "240": PotionID = "017"; break;
                case "120": PotionID = "016"; break;
                case "Crisis": PotionID = "072"; break;
                default: return;
            }

            if (FindMPPotion() != "" & !pet)
            {
                SendPacket("3103" + AlignDWORD(int.Parse("490" + PotionID))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000000");
            }
            else
                SendPacket("760203" + AlignDWORD(int.Parse("490" + PotionID))[..8] + AlignDWORD(GetPetId())[..8] + AlignDWORD(GetPetId())[..8] + "000000000000000000000000000000000000000000000000");
        }

        public static void Disconnect()
        {
            try
            {
                Process KO = Process.GetProcessById(GamePID);
                KO.Kill();
            }
            catch { }
        }

        public static int SearchMob(ref List<TargetInfo> OutTargetList)
        {
            return SearchTarget(0x28, ref OutTargetList);
        }

        public static int SearchPlayer(ref List<TargetInfo> OutTargetList)
        {
            return SearchTarget(0x30, ref OutTargetList);
        }

        public static int SearchTarget(int Address, ref List<TargetInfo> OutTargetList)
        {
            int Ebp = Read4Byte(Read4Byte(KO_PTR_FLDB) + Address);
            int Fend = Read4Byte(Read4Byte(Ebp + 0x4) + 0x4);
            int Esi = Read4Byte(Ebp);
            int Tick = Environment.TickCount;

            while (Esi != Ebp && Environment.TickCount - Tick < 50)
            {
                int Base = Read4Byte(Esi + 0x14);

                if (Base == 0)
                {
                    break;
                }

                if (OutTargetList.Any(x => x.Base == Base) == false)
                {
                    TargetInfo Target = new()
                    {
                        Base = Base,
                        X = GetTargetX(Base),
                        Y = GetTargetY(Base),
                        Id = GetTargetId(Base),
                        Name = GetTargetName(Base),
                        Hp = GetTargetHealth(Base),
                        State = GetTargetState(Base),
                        MaxHp = GetTargetMaxHealth(Base),
                        Nation = GetTargetRaceType(Base),
                        MoveType = GetTargetMoveType(Base),
                    };

                    OutTargetList.Add(Target);
                }

                int Eax = Read4Byte(Esi + 8);

                if (Eax != Fend)
                {
                    while (Read4Byte(Eax) != Fend && Environment.TickCount - Tick < 50)
                    {
                        Eax = Read4Byte(Eax);
                    }
                    Esi = Eax;
                }
                else
                {
                    Eax = Read4Byte(Esi + 4);

                    while (Esi == Read4Byte(Eax + 8) && Environment.TickCount - Tick < 50)
                    {
                        Esi = Eax;
                        Eax = Read4Byte(Eax + 4);
                    }

                    if (Read4Byte(Esi + 8) != Eax)
                    {
                        Esi = Eax;
                    }
                }
            }

            return OutTargetList.Count;
        }

        public static void SelectTarget(int TargetId)
        {
            if (TargetId > 0)
                SendPacket("22" + AlignDWORD(TargetId)[..8] + "01");

            Write4Byte(Read4Byte(KO_PTR_CHR) + KO_OFF_MOB, TargetId);
        }

        public static bool NeedRPR()
        {
            for (int i = 1; i < 14; i++)
            {
                switch (i)
                {
                    case 1:
                    case 4:
                    case 6:
                    case 8:
                    case 10:
                    case 12:
                    case 13:
                        {
                            if (IsInventorySlotEmpty(i) == false && GetItemDurability(i) == 0)
                                return true;
                        }
                        break;
                }
            }
            return false;
        }

        public static bool NeedSupply(int itemID)
        {
            if (GetItemsInInventory(itemID) == false || CheckItemCount(GetInventoryItemSlot(itemID)) < 5)
                return true;

            return false;
        }

        public static void GetMyItemsRepair(int npcID)
        {
            for (int i = 1; i < 14; i++)
            {
                switch (i)
                {
                    case 1:
                    case 4:
                    case 6:
                    case 8:
                    case 10:
                    case 12:
                    case 13:
                        {
                            if (IsInventorySlotEmpty(i) == false)
                            {
                                SendPacket("3B01" + AlignDWORD(i)[..2] + AlignDWORD(npcID)[..8] + AlignDWORD(GetItemId(i)));
                                Thread.Sleep(50);
                            }
                        }
                        break;
                }
            }
        }

        public static void BuyItems(int itemID, int Npc, int Count, int buyPacketCountSize, int buyPacketEnd, int ExecutionAfterWait = 0)
        {
            if (Count == 0)
                return;

            int InventoryItemSlot = GetInventoryItemSlot(itemID);

            InventoryItemSlot = InventoryItemSlot != -1 ? InventoryItemSlot : GetInventoryEmptySlot();

            if (InventoryItemSlot == -1)
                return;

            string ItemCount = buyPacketCountSize == 0 ? AlignDWORD(Count) : AlignDWORD(Count)[..buyPacketCountSize];

            SendPacket("2101" + "18E40300" + AlignDWORD(Npc)[..8] + "01" + AlignDWORD(itemID) + AlignDWORD(InventoryItemSlot - 14)[..2] + ItemCount + AlignDWORD(buyPacketEnd)[..2]);

            Thread.Sleep(50);
            SendPacket("6A02");

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public static void SellItem(int ItemID, int Npc, int Count, int Slot, int ExecutionAfterWait = 0)
        {
            if (Count == 0)
                return;

            string SellPacket = "";

            switch (GetZoneId())
            {
                case 11: SellPacket = "18E40300"; break;
                case 21: SellPacket = "11150300"; break;
            }

            SendPacket("2102" + SellPacket + AlignDWORD(Npc)[..8] + "01" + AlignDWORD(ItemID) + AlignDWORD(Slot - 14)[..2] + AlignDWORD(Count)[..4]);
            SendPacket("6A02");

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }
    }
}

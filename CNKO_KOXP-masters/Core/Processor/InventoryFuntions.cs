using KOXP.Constants.Addresses;
using static KOXP.Constants.Address;
using static KOXP.Core.Processor.CharFunctions;

namespace KOXP.Core.Processor
{
    public class InventoryFunctions : AddressExtensions
    {
        public static int GetItemId(int Slot)
        {
            int ID = Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(KO_PTR_DLG) + 0x1A8) + 0x210 + (4 * Slot)) + 0x68));
            int EXT = Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(KO_PTR_DLG) + 0x1A8) + 0x210 + (4 * Slot)) + 0x6C));

            return ID + EXT;
        }

        public static int CheckItemCount(int Slot)
        {
            return Read4Byte(Read4Byte(Read4Byte(Read4Byte(KO_PTR_DLG) + 0x1A8) + 0x210 + (4 * Slot)) + 0x70);
        }

        public static int GetItemDurability(int i)
        {
            int Base = Read4Byte(Read4Byte(Read4Byte(KO_PTR_DLG) + 0x1A8) + 0x210 + (4 * i));
            int result;

            if (Base > 0)
                result = Read4Byte(Base + 0x74);
            else
                return 0;

            return result;
        }

        public static int GetInventoryItemSlotFast(int itemID, int i)
        {
            return itemID != 0 && itemID == GetItemId(i) ? i : -1;
        }

        public static int GetInventoryItemSlot(int ItemID)
        {
            int InventoryBase = Read4Byte(Read4Byte(KO_PTR_DLG) + 0x1A8);
            for (int i = 14; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + 0x210 + (4 * i));

                if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == ItemID)
                    return i;
            }

            return -1;
        }

        public static string GetInventoryItemName(int SlotId)
        {
            int InventoryBase = Read4Byte(Read4Byte(KO_PTR_DLG) + 0x1A8);
            int Length = Read4Byte(InventoryBase + 0x210 + (4 * SlotId));
            int Name = Read4Byte(Length + 0x68);
            int NameLength = Read4Byte(Name + 0x1C);

            return NameLength > 15 ? ReadString(Read4Byte(Name + 0xC), NameLength) : ReadString(Name + 0xC, NameLength);
        }

        public static bool GetItemsInInventory(int itemID)
        {
            for (int i = 14; i < 42; i++)
                if (GetItemId(i) == itemID)
                    return true;

            return false;
        }

        public static bool IsInventorySlotEmpty(int Slot)
        {
            return GetItemId(Slot) == 0;
        }

        public static int GetInventoryEmptySlot()
        {
            for (int i = 14; i < 42; i++)
                if (GetItemId(i) == 0)
                    return i;

            return -1;
        }

        public static bool InventoryIsFull()
        {
            if (GetMaxWeight() - GetWeight() <= 10)
                return true;

            for (int i = 14; i < 42; i++)
                if (GetItemId(i) == 0)
                    return false;

            return true;
        }
    }
}

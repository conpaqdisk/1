using KOXP.Common;
//using KOXP.Data;
using System.Diagnostics;
using static KOXP.Constants.Addresses.AddressHandler;
using static KOXP.Constants.Handle;
using static KOXP.Constants.Id;
using static KOXP.Core.Helper;
using static KOXP.Core.Processor.CharFunctions;
using static KOXP.Core.Processor.Functions;
using static KOXP.Core.Processor.InventoryFunctions;
using static KOXP.Core.Processor.SkillFunctions;
using static KOXP.Core.Processor.Skills;
using static KOXP.Win32.Win32Api;

namespace KOXP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //bool applicationReady = false;
        List<PRequestList>? timeAfterPartyRequest = new() { new PRequestList() };
        #region "Public Variables"
        public static string? Job { get; set; }
        public static (string, int, int)[]? attackSkills { get; set; }
        public static (string, int, int, int)[]? timedSkills { get; set; }
        public static (string, int, int, int)[]? healSkills { get; set; }
        public static long timeAfterAttack { get; set; }
        public static long timeAfterProtectionEvent { get; set; }
        public static long timeAfterPHealEvent { get; set; }
        public static long timeAfterSkillCheck { get; set; }
        public static int attackRange { get; set; }
        public static int targetSelectRange { get; set; }
        public static int Delay { get; set; }
        public static int waitTimeAfterTrainingAreaReturn { get; set; } = 0;
        public static int centerX { get; set; }
        public static int centerY { get; set; }
        #endregion

        private void txtWindowsName_DropDown(object sender, EventArgs e)
        {
            txtWindowsName.Items.Clear();
            Process[] Process = System.Diagnostics.Process.GetProcessesByName("KnightOnLine");

            for (int i = 0; i < Process.Length; i++)
            {
                txtWindowsName.Items.Add(Process[i].MainWindowTitle);
            }
        }

        private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAlwaysOnTop.Checked)
                SetWindowPos(new IntPtr(this.Handle.ToInt32()), new IntPtr(-1), this.Left, this.Top, this.Width, this.Height, (uint)0x2);
            else
                SetWindowPos(new IntPtr(this.Handle.ToInt32()), new IntPtr(-2), this.Left, this.Top, this.Width, this.Height, (uint)0x2);
        }

        private void chkWh_CheckedChanged(object sender, EventArgs e)
        {
            Wallhack(chkWh.Checked);
        }

        private void chkLoot_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLoot.Checked)
                GetOread();
        }

        private void btnLoot_Click(object sender, EventArgs e)
        {
            SendPacket("1F0103" + AlignDWORD(GetItemId(41)) + AlignDWORD(27)[..2] + AlignDWORD(35)[..2]);
            SendPacket("6A02");
        }

        private void btnMs_Click(object sender, EventArgs e)
        {
            GetMonsterStones();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lstSlot.Items.Clear();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstSlot.SelectedItems.Count > 0)
                lstSlot.Items.RemoveAt(lstSlot.SelectedIndex);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string targetName = TargetName();
            if (targetName != "" && !lstSlot.Items.Contains(targetName))
                lstSlot.Items.Add(targetName);
        }

        private void btnAddNearby_Click(object sender, EventArgs e)
        {
            List<TargetInfo> TargetList = new();

            if (SearchMob(ref TargetList) > 0)
                TargetList.ForEach(x =>
                {
                    if (x.Base > 0 && x.Name != null && x.Nation == 0 && !lstSlot.Items.Contains(x.Name))
                        lstSlot.Items.Add(x.Name);
                });
        }

        private void txtTargetDist_ValueChanged(object sender, EventArgs e)
        {
            targetSelectRange = (int)txtTargetDist.Value;
        }

        private void txtAttackRange_ValueChanged(object sender, EventArgs e)
        {
            attackRange = (int)txtAttackRange.Value;
        }
        private void chkBackToCenter_CheckedChanged(object sender, EventArgs e)
        {
            timerBackToCenter.Enabled = chkBackToCenter.Checked;
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            try
            {
                bool isParsedX = int.TryParse(txtX.Text, out int X);
                bool isParsedY = int.TryParse(txtY.Text, out int Y);

                GoToTarget(X, Y);
            }
            catch { }
        }

        private void timerBackToCenter_Tick(object sender, EventArgs e)
        {
            if (GetAction() == EAction.Attack)
            {
                if (Distance(TargetX(), TargetY(), centerX, centerY) > txtTargetDist.Value ||
                    Distance(GetX(), GetY(), centerX, centerY) > txtTargetDist.Value)
                {
                    SelectTarget(0);
                    GoToTarget(centerX, centerY);
                }
            }
        }

        private void timerBuffSc_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < lstBuffSc.CheckedItems.Count; i++)
                BuffSC(lstBuffSc.GetItemText(lstBuffSc.CheckedItems[i]));

        }

        private void timerMinor_Tick(object sender, EventArgs e)
        {
            try
            {
                if (GetHp() <= GetMaxHp() * (int)txtMinorHeal.Value / 100 && GetMp() > 50)
                    MinorHeal();
            }
            catch { }
        }

        private void timerEscape_Tick(object sender, EventArgs e)
        {
            try
            {
                if (GetHp() <= GetMaxHp() * (int)txtEscape.Value / 100)
                {
                    SendPacket("3103" + AlignDWORD(490360)[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000000");
                    Thread.Sleep(250);
                    SendPacket("3106" + AlignDWORD(490360)[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000");
                    SendPacket("1200");
                }
            }
            catch { }
        }

        private void timerSupplyControl_Tick(object sender, EventArgs e)
        {
            if (GetAction() == EAction.Attack && GetZoneId() == KarusEslant && Environment.TickCount - waitTimeAfterTrainingAreaReturn > 15000)
            {
                if (chkSellAllItems.Checked && InventoryIsFull() ||
                    chkRpr.Checked && NeedRPR() ||
                    chkArrow.Checked && NeedSupply(Arrow) ||
                    chkWolf.Checked && NeedSupply(Wolf) ||
                    chkBook.Checked && NeedSupply(Book))
                {
                    SetAction(EAction.Supplying);
                    MakeASupply();
                }
            }
        }

        private void MakeASupply()
        {
            Thread Supply = new(() =>
            {
                int TrainingAreaX = GetX();
                int TrainingAreaY = GetY();
                int NpcId = 0;
                int NpcX = 0;
                int NpcY = 0;
                bool IsNpcInTown = false;

                switch (GetZoneId())
                {
                    case 11:
                        {
                            IsNpcInTown = true;
                            NpcId = EslantSundries;
                            NpcX = 540; NpcY = 565;
                            break;
                        }
                }

                if (IsNpcInTown)
                    SendPacket("4800");

                txtStatus.ForeColor = Color.Red;
                txtStatus.Text = "Tedarik icin gidiliyor.";

                while (GetAction() is EAction.Supplying)
                {
                    if (txtStatus.Text == "Tedarik icin gidiliyor.")
                    {
                        if (GoToTarget(NpcX, NpcY))
                        {
                            txtStatus.Text = "Tedarik islemi basladi.";

                            if (chkRpr.Checked)
                                GetMyItemsRepair(NpcId);

                            if (chkSellAllItems.Checked)
                            {
                                for (int i = 14; i < 42; i++)
                                {
                                    if (GetItemId(i) != 0 && GetItemId(i) != Book && GetItemId(i) != Arrow && GetItemId(i) != Wolf && GetItemId(i) != 379021000 && GetItemId(i) != 379025000 && GetItemId(i) != 111110001 && GetItemId(i) != 111210001 && GetItemId(i) != 156210000)
                                        SellItem(GetItemId(i), NpcId, CheckItemCount(i), i, 50);
                                }
                            }

                            if (chkArrow.Checked)
                                BuyItems(Arrow, NpcId, int.Parse(txtArrow.Text) - CheckItemCount(GetInventoryItemSlot(Arrow)), 0, 250);

                            if (chkWolf.Checked)
                                if (CheckItemCount(GetInventoryItemSlot(Wolf)) < (int)txtWolf.Value)
                                    BuyItems(Wolf, NpcId, (int)txtWolf.Value - CheckItemCount(GetInventoryItemSlot(Wolf)), 7, 125);

                            if (chkBook.Checked)
                                if (CheckItemCount(GetInventoryItemSlot(Book)) < (int)txtBook.Value)
                                    BuyItems(Book, NpcId, (int)txtBook.Value - CheckItemCount(GetInventoryItemSlot(Book)), 6, 11);

                            txtStatus.Text = "Slota gidiliyor.";
                        }
                    }

                    if (txtStatus.Text == "Slota gidiliyor.")
                    {
                        if (GoToTarget(TrainingAreaX, TrainingAreaY))
                        {
                            Thread.Sleep(250);
                            SetAction(EAction.Attack);
                            txtStatus.ForeColor = Color.Green;
                            txtStatus.Text = "OK";
                            waitTimeAfterTrainingAreaReturn = Environment.TickCount;
                        }
                    }
                }
            })
            {
                IsBackground = true
            };
            Supply.Start();
        }

        private void timerInfo_Tick(object sender, EventArgs e)
        {
            try
            {
                int Hp = GetHp() * 100 / GetMaxHp();
                int Mp = GetMp() * 100 / GetMaxMp();
                float Exp = GetExp() * 100 / GetMaxExp();

                barHp.Value = Hp;
                barMp.Value = Mp;
                barExp.Value = Convert.ToInt32(Exp);

                txtPerHp.Text = $"%{Hp}";
                txtPerMp.Text = $"%{Mp}";

                txtPerExp.Text = string.Format("%{0:N2}", Exp);

                if (chkDc.Checked && DC())
                {
                    txtStatus.Text = "Disconnect";
                    txtStatus.ForeColor = Color.Red;
                    Disconnect();
                }

            }
            catch { }
        }

        private void timerAttackR_Tick(object sender, EventArgs e)
        {
            if (GetAction() == EAction.Attack)
            {
                int TargetId = GetTargetId();

                if (TargetId > 0)
                    SendPacket("080101" + AlignDWORD(TargetId)[..8] + "FF000000" + "0000");
            }
        }

        private void timerPotion_Tick(object sender, EventArgs e)
        {
            if (chkPotHp.Checked && cmbPotHp.Text != null && GetHp() < (GetMaxHp() * int.Parse(txtPotHp.Text) / 100))
                PotionHP(cmbPotHp.Text, false);

            if (chkPotMp.Checked && cmbPotMp.Text != null && GetMp() < (GetMaxMp() * int.Parse(txtPotMp.Text) / 100))
                PotionMP(cmbPotMp.Text, false);
        }

        public void CheckBoxControls()
        {
            Wallhack(chkWh.Checked);

            if (chkLoot.Checked)
                GetOread();
        }

        private void JobControl()
        {
            if (Job == null)
                return;

            switch (Job)
            {
                case "Rogue": txtAttackRange.Value = 40; attackRange = 40; targetSelectRange = 40; Delay = 1250; break;
                case "Mage": txtAttackRange.Value = 35; attackRange = 35; targetSelectRange = 35; Delay = 1250; break;
                case "Warrior": txtAttackRange.Value = 10; attackRange = 10; targetSelectRange = 40; Delay = 800; break;
                case "Priest": txtAttackRange.Value = 10; attackRange = 10; targetSelectRange = 40; Delay = 800; break;
            }
        }

        public void LoadSkills()
        {
            lstBuffSc.Items.Clear();
            lstBuffSc.Items.AddRange(new object[]
            { "Hyper Noah", "AP 120 min", "Def SC", "AP 30 min", "Pink", "Gray", "Green", "Rich Merchant" });

            string Class = GetClass().ToString();

            if (Job == null)
                return;

            if (Job.Equals("Rogue"))
            {
                attackSkills = new (string, int, int)[]
                {
                    ("Super Archer", 0, 0),
                    ("Archery", 0, 0),
                    ("Through Shot", 0, 0),
                    ("Fire Arrow", 3, 3),
                    ("Poison Arrow", 3, 3),
                    ("Multiple Shot", 0, 0),
                    ("Guided Arrow", 0, 0),
                    ("Perfect Shot", 0, 0),
                    ("Fire Shot", 4, 4),
                    ("Poison Shot", 4, 4),
                    ("Arc Shot", 0, 0),
                    ("Explosive Shot", 4, 4),
                    ("Counter Strike", 60, 60),
                    ("Arrow Shower", 0, 0),
                    ("Shadow Shot", 0, 0),
                    ("Shadow Hunter", 0, 0),
                    ("Ice Shot", 6, 6),
                    ("Lightning Shot", 6, 6),
                    ("Dark Pursuer", 0, 0),
                    ("Blow Arrow", 0, 0),
                    ("Blinding Strafe", 60, 60),

                    ("Stroke", 0, 0),
                    ("Stab", 6, 6),
                    ("Stab2", 6, 6),
                    ("Jab", 6, 6),
                    ("Pierce", 11, 11),
                    ("Shock", 6, 6),
                    ("Thrust", 11, 11),
                    ("Cut", 6, 6),
                    ("Spike", 12, 12),
                    ("Blody Beast", 6, 6),
                    ("Blinding", 60, 60)
                };

                timedSkills = new (string, int, int, int)[]
                {
                ("Sprint", 6, 6,int.Parse(Class + "001")),
                ("Wolf", 0, 0,int.Parse(Class + "030")),
                ("Swift", 0, 0, int.Parse(Class + "010")),
                ("Light Feet", 10, 10, int.Parse(Class + "725")),
                ("Evade", 30, 30, int.Parse(Class + "710")),
                ("Safely", 30, 30, int.Parse(Class + "730")),
                ("Scaled Skin", 30, 30, int.Parse(Class + "760")),
                ("Lupin Eyes", 9, 9, int.Parse(Class + "735")),
                ("Hide", 6, 6, int.Parse(Class + "700"))
                };
            }

            if (Job.Equals("Warrior"))
            {
                attackSkills = new (string, int, int)[]
                {
                    ("Stroke", 0, 0),
                    ("Slash", 3, 3),
                    ("Crash", 3, 3),
                    ("Piercing", 3, 3),
                    ("Hash", 3, 3),
                    ("Hoodwink", 0, 0),
                    ("Shear", 3, 3),
                    ("Pierce", 0, 0),
                    ("Leg Cutting", 5, 5),
                    ("Carving", 0, 0),
                    ("Sever", 3, 3),
                    ("Prick",0,0),
                    ("Multiple Shock", 3,3),
                    ("Cleave",0,0),
                    ("Mangling",0,0),
                    ("Thrust",0,0),
                    ("Sword Aura",0,0),
                    ("Sword Dancing",0,0),
                    ("Howling Sword",1,1),
                    ("Blooding",21,21),
                    ("Hell Blade",1,1),
                    ("Provoke",15,15)
                };

                timedSkills = new (string, int, int, int)[]
                {
                    ("Sprint", 6, 6, int.Parse(Class + "002")),
                    ("Defence", 10, 10, int.Parse(Class + "007"))
                };
            }

            if (Job.Equals("Mage"))
            {
                attackSkills = new (string, int, int)[]
                {
                    ("Stroke", 1, 1),
                    ("Flash", 4, 4),
                    ("Shiver", 4, 4),
                    ("Flame", 4, 4),
                    ("Cold Wave", 4, 4),
                    ("Spark", 4, 4),
                    ("Burn", 1, 1),
                    ("Blaze", 6, 6),
                    ("Fire Ball", 5, 5),
                    ("Ignition", 1, 1),
                    ("Fire Spear", 5, 5),
                    ("Fire Burst", 0, 0),
                    ("Fire Blast", 5, 5),
                    ("Hell Fire", 5, 5),
                    ("Fire Blade", 1, 1),
                    ("Specter Of Fire", 1, 1),
                    ("Inferno", 16, 16),
                    ("Pillar Of Fire", 6, 6),
                    ("Manes Of Fire", 1, 1),
                    ("Fire Impact", 21, 21),
                    ("Super Nova", 16, 16),
                };
            }

            if (Job.Equals("Priest"))
            {
                attackSkills = new (string, int, int)[]
                {
                    ("Stroke", 0, 0),
                    ("Holy Attack", 0, 0),
                    ("Collision", 0, 0),
                    ("Wrath", 0, 0),
                    ("Tilt", 0, 0),
                    ("Shuddering", 0, 0),
                    ("Wield", 0, 0),
                    ("Bloody", 0, 0),
                    ("Ruin", 2, 2),
                    ("Harsh", 2, 2),
                    ("Raving Edge", 2, 2),
                    ("Hellish", 3, 3),
                    ("Collapse", 3, 3),
                    ("Hades", 3, 3),
                    ("Judgment", 0, 0),
                    ("Helis", 0, 0),
                };

                timedSkills = new (string, int, int, int)[]
                {
                    ("Prayer of God's Power", 6, 6, int.Parse(Class + "020")),
                };

                healSkills = new (string, int, int, int)[]
                {
                    ("Minor Healing",0,0,int.Parse(Class + "500")),
                    ("Light Restore",0,0,int.Parse(Class + "503")),
                    ("Healing",0,0,int.Parse(Class + "509")),
                    ("Restore",0,0,int.Parse(Class + "512")),
                    ("Major Healing",0,0,int.Parse(Class + "518")),
                    ("Major Restore",0,0,int.Parse(Class + "521")),
                    ("Great Healing",0,0,int.Parse(Class + "527")),
                    ("Great Restore",0,0,int.Parse(Class + "530")),
                    ("Massive Healing",0,0,int.Parse(Class + "536")),
                    ("Massive Restore",0,0,int.Parse(Class + "539")),
                    ("Superior Healing",0,0,int.Parse(Class + "545")),
                    ("Superior Restore",0,0,int.Parse(Class + "548")),
                    ("Complete Healing",6,6,int.Parse(Class + "554")),
                    ("Group Massive Healing",6,6,int.Parse(Class + "557")),
                    ("Group Complete Healing",7,7,int.Parse(Class + "560")),
                    ("Critical Restore",0,0,int.Parse(Class + "570")),
                    ("Past Recovery",0,0,int.Parse(Class + "575")),
                    ("Past Restore",120,120,int.Parse(Class + "580")),
                };

                lstHealSkills.Items.Clear();
                for (int i = 0; i < healSkills.Length; i++)
                {
                    lstHealSkills.Items.AddRange(new object[]
                    {
                        healSkills[i].Item1
                    });
                }
            }

            if (attackSkills == null)
                return;

            lstSkills.Items.Clear();

            for (int i = 0; i < attackSkills.Length; i++)
            {
                lstSkills.Items.AddRange(new object[]
                {
                    attackSkills[i].Item1
                });
            }

            if (timedSkills == null)
                return;

            lstTimedSkills.Items.Clear();
            
            for (int i = 0; i < timedSkills.Length; i++)
            {
                lstTimedSkills.Items.AddRange(new object[]
                {
                        timedSkills[i].Item1
                });
            }
        }

        private void TargetSearchAndSelect()
        {
            try
            {
                while (true)
                {
                    if (GetAction() == EAction.Attack && chkAutoZ.Checked)
                    {
                        int Base = GetTargetBase(GetTargetId());

                        if (GetTargetId() > 0)
                        {
                            if (Base == 0 || GetTargetMoveType(Base) == 4)
                                SelectTarget(0);
                        }
                        else
                        {
                            List<TargetInfo> TargetList = new();
                            SearchMob(ref TargetList);

                            if (TargetList.Count == 0)
                            {
                                SelectTarget(0);
                                Thread.Sleep(100);
                            }
                            else
                            {
                                if (lstSlot.CheckedItems.Count > 0 && chkBackToCenter.Checked)
                                {
                                    TargetInfo? Target = TargetList.FindAll(x =>
                                    Distance(x.X, x.Y, centerX, centerY) <
                                    txtTargetDist.Value && x.Name != null &&
                                    IsSelectableTargetWithBase(x.Base) &&
                                    lstSlot.CheckedItems.Contains(x.Name))
                                    .GroupBy(x => Math.Pow(GetX() - x.X, 2) + Math.Pow(GetY() - x.Y, 2))
                                    .OrderBy(x => x.Key)
                                    ?.FirstOrDefault()
                                    ?.FirstOrDefault();

                                    if (Target != null)
                                        SelectTarget(Target.Id);
                                    else
                                        SelectTarget(0);
                                }
                                else if (lstSlot.CheckedItems.Count > 0 && chkBackToCenter.Checked == false)
                                {
                                    TargetInfo? TargetWithName = TargetList.FindAll(x =>
                                    Distance(x.X, x.Y, GetX(), GetY()) <
                                    txtTargetDist.Value && x.Name != null &&
                                    IsSelectableTargetWithBase(x.Base) &&
                                    lstSlot.CheckedItems.Contains(x.Name))
                                    .GroupBy(x => Math.Pow(GetX() - x.X, 2) + Math.Pow(GetY() - x.Y, 2))
                                    .OrderBy(x => x.Key)
                                    ?.FirstOrDefault()
                                    ?.FirstOrDefault();

                                    if (TargetWithName != null)
                                        SelectTarget(TargetWithName.Id);
                                    else
                                        SelectTarget(0);
                                }
                                else if (lstSlot.CheckedItems.Count == 0 && chkBackToCenter.Checked)
                                {
                                    TargetInfo? Target = TargetList.FindAll(x =>
                                    Distance(x.X, x.Y, centerX, centerY) <
                                    txtTargetDist.Value &&
                                    IsSelectableTargetWithBase(x.Base))
                                    .GroupBy(x => Math.Pow(GetX() - x.X, 2) + Math.Pow(GetY() - x.Y, 2))
                                    .OrderBy(x => x.Key)
                                    ?.FirstOrDefault()
                                    ?.FirstOrDefault();

                                    if (Target != null)
                                        SelectTarget(Target.Id);
                                    else
                                        SelectTarget(0);
                                }
                                else if (lstSlot.CheckedItems.Count == 0 && chkBackToCenter.Checked == false)
                                {
                                    TargetInfo? Target = TargetList.FindAll(x =>
                                        Distance(x.X, x.Y, GetX(), GetY()) <
                                        txtTargetDist.Value &&
                                        IsSelectableTargetWithBase(x.Base))
                                        .GroupBy(x => Math.Pow(GetX() - x.X, 2) + Math.Pow(GetY() - x.Y, 2))
                                        .OrderBy(x => x.Key)
                                        ?.FirstOrDefault()
                                        ?.FirstOrDefault();

                                    if (Target != null)
                                        SelectTarget(Target.Id);
                                    else
                                        SelectTarget(0);
                                }
                            }
                        }

                        if (chkRunToTarget.Checked && IsSelectableTargetWithBase(Base))
                            GoToTarget(GetTargetX(Base), GetTargetY(Base));
                    }
                    else
                        Thread.Sleep(10);
                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void TimedSkillEvent()
        {
            try
            {
                while (true)
                {
                    if (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - timeAfterSkillCheck >= 1000)
                    {
                        if (timedSkills == null)
                            return;

                        string Skill = ChoosenTimedSkill(lstTimedSkills, timedSkills);

                        if (Skill != "" && btnEnabled.Text.Equals("Devre disi"))
                        {
                            UseSkillWhileWalking();
                            UseTimedSkill(Skill);
                        }

                        timeAfterSkillCheck = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    }
                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private string ChoosenTimedSkill(CheckedListBox SkillList, (string, int, int, int)[] Skills)
        {
            int Choosen = -1; int Delay = -1;

            for (int i = 0; i < SkillList.Items.Count; i++)
            {
                if (SkillList.GetItemChecked(i))
                {
                    if (Skills[i].Item2 >= Skills[i].Item3 && Skills[i].Item2 >= Delay && !SkillOverlap(Skills[i].Item1) && !IsSkillInUsed(Skills[i].Item4))
                    {
                        Choosen = i;
                        Delay = Skills[i].Item2;
                        Skills[Choosen].Item2 = 0;
                    }
                    else
                        Skills[i].Item2 += 1;
                }
                else
                    Skills[i].Item2 += 1;
            }

            if (Choosen == -1)
                return "";

            try { return Skills[Choosen].Item1; }
            catch { return ""; }
        }

        private string ChoosenAttackSkill(CheckedListBox SkillList, (string, int, int)[] Skills)
        {
            int Choosen = -1; int Delay = -1;

            for (int i = 0; i < SkillList.Items.Count; i++)
            {
                if (SkillList.GetItemChecked(i))
                {
                    if (Skills[i].Item2 >= Skills[i].Item3 && Skills[i].Item2 >= Delay)
                    {
                        Choosen = i;
                        Delay = Skills[i].Item2;
                    }
                    else
                        Skills[i].Item2 += 1;
                }
                else
                    Skills[i].Item2 += 1;
            }

            if (Choosen == -1)
                return "";

            int Base = GetMobBase(GetTargetId());

            if (IsAttackableTargetWithBase(Base))
                try { Skills[Choosen].Item2 = 0; } catch { return ""; }

            try { return Skills[Choosen].Item1; }
            catch { return ""; }
        }

        public void AttackEvent()
        {
            try
            {
                while (true)
                {
                    if (GetAction() == EAction.Attack && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - timeAfterAttack >= Delay)
                    {
                        if (attackSkills == null)
                            continue;

                        string Skill = ChoosenAttackSkill(lstSkills, attackSkills);

                        if (Skill != null && Skill != "")
                        {
                            UseSkillWhileWalking();

                            if (MakeAttack(Skill, GetTargetId()))
                                timeAfterAttack = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        }
                    }
                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void ProtectionEvent()
        {
            try
            {
                while (true)
                {
                    if (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - timeAfterProtectionEvent >= 200)
                    {
                        if (chkGodMode.Checked)
                            if (GetHp() <= (GetMaxHp() * txtGodModeHp.Value / 100) || GetMp() <= (GetMaxMp() * txtGodModeMp.Value / 100))
                            {
                                SendPacket("3106" + AlignDWORD(500344)[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000");
                                Thread.Sleep(5);
                                SendPacket("3103" + AlignDWORD(500344)[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000000");
                            }

                        if (chkMinorHeal.Checked)
                            if (GetHp() <= GetMaxHp() * (int)txtMinorHeal.Value / 100 && GetMp() > 50)
                                MinorHeal();

                        timeAfterProtectionEvent = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    }
                    Thread.Sleep(1);
                }
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private int ChoosenHealSkill(CheckedListBox SkillList, (string, int, int, int)[] Skills)
        {
            int Choosen = -1; int Delay = -1;

            for (int i = 0; i < SkillList.Items.Count; i++)
            {
                if (SkillList.GetItemChecked(i))
                {
                    if (Skills[i].Item2 >= Skills[i].Item3 && Skills[i].Item2 >= Delay && !SkillOverlap(Skills[i].Item1) && !IsSkillInUsed(Skills[i].Item4))
                    {
                        Choosen = i;
                        Delay = Skills[i].Item2;
                        Skills[Choosen].Item2 = 0;
                    }
                    else
                        Skills[i].Item2 += 1;
                }
                else
                    Skills[i].Item2 += 1;
            }

            if (Choosen == -1)
                return -1;

            try { return Skills[Choosen].Item4; }
            catch { return -1; }
        }

        private void PriestHealEvent()
        {
            try
            {
                while (true)
                {
                    if (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - timeAfterPHealEvent >= 1000)
                    {
                        if (healSkills == null)
                            return;

                        int cHealSkill = ChoosenHealSkill(lstHealSkills, healSkills);

                        if (cHealSkill != -1 && GetHp() <= GetMaxHp() * (int)txtHeal.Value / 100)
                        {
                            UseSkillWhileWalking();
                            UseHealSkill(cHealSkill);
                        }

                        timeAfterPHealEvent = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    }
                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        public static bool MakeAttack(string Skill, int TargetId)
        {
            switch (Job)
            {
                case "Rogue": RogueAttack(Skill, TargetId); return true;
                case "Mage": MageAttack(Skill, TargetId); return true;
                case "Warrior": WarriorAttack(Skill, TargetId); return true;
                case "Priest": PriestAttack(Skill, TargetId); return true;
            }
            return false;
        }

        private void StartKoxp()
        {
            //if (applicationReady != true)
            //{
            //    MessageBox.Show("Uygulama yukleniyor.");
            //    return;
            //}s

            if (GetId() != 0)
            {
                btnInject.ForeColor = Color.Green;

                txtStatus.Text = "OK";
                txtStatus.ForeColor = Color.Green;

                Text = $"[{GetName()}]";

                Job = GetJob();

                LoadSkills();
                JobControl();

                CheckBoxControls();

                StartThread(AttackEvent);
                StartThread(TargetSearchAndSelect);
                StartThread(TimedSkillEvent);
                StartThread(ProtectionEvent);

                if (Job.Equals("Priest"))
                    StartThread(PriestHealEvent);

                timerInfo.Enabled = true;
                timerBuffSc.Enabled = true;
            }
            else
            {
                MessageBox.Show("Karakter oyuna girdikten sonra koxp'u baglayin");
                btnInject.ForeColor = Color.Red;
            }
        }

        private void btnInject_Click(object sender, EventArgs e)
        {
            if (AttachProccess(txtWindowsName.Text))
                StartKoxp();
        }

        private void btnStartAttack_Click(object sender, EventArgs e)
        {
            if (btnStartAttack.Text.Equals("Atak baslat"))
            {
                btnStartAttack.Text = "Atak durdur";
                btnStartAttack.ForeColor = Color.Red;

                SetAction(EAction.Attack);

                if (chkBackToCenter.Checked)
                {
                    centerX = GetX();
                    centerY = GetY();

                    lblCenterX.Text = centerX.ToString();
                    lblCenterY.Text = centerY.ToString();

                    timerBackToCenter.Enabled = true;
                }

                if (GetJob() == "Warrior" || GetJob() == "Priest")
                    timerAttackR.Enabled = true;
            }
            else
            {
                btnStartAttack.Text = "Atak baslat";
                btnStartAttack.ForeColor = Color.Green;

                SetAction(EAction.None);

                lblCenterX.Text = "0";
                lblCenterY.Text = "0";

                timerBackToCenter.Enabled = false;

                timerAttackR.Enabled = false;
            }
        }

        private void btnTown_Click(object sender, EventArgs e)
        {
            SendPacket("4800");
        }

        private void btnEnabled_Click(object sender, EventArgs e)
        {
            if (btnEnabled.Text.Equals("Etkinlestir"))
            {
                timerBuffSc.Enabled = true;

                btnEnabled.Text = "Devre disi";
                btnEnabled.ForeColor = Color.Red;
            }
            else
            {
                timerBuffSc.Enabled = false;

                btnEnabled.Text = "Etkinlestir";
                btnEnabled.ForeColor = Color.Green;
            }
        }

        private void cmbPotHp_DropDown(object sender, EventArgs e)
        {
            cmbPotHp.Items.Clear();

            cmbPotHp.Items.Add("Automatic");

            for (int i = 14; i < 42; i++)
            {
                switch (GetItemId(i))
                {
                    case 389015000: cmbPotHp.Items.Add("1440"); break;
                    case 389014000: cmbPotHp.Items.Add("720"); break;
                    case 389013000: cmbPotHp.Items.Add("360"); break;
                    case 389012000: cmbPotHp.Items.Add("180"); break;
                    case 389011000: cmbPotHp.Items.Add("90"); break;
                    case 389010000: cmbPotHp.Items.Add("45"); break;
                }
            }
        }

        private void cmbPotMp_DropDown(object sender, EventArgs e)
        {
            cmbPotMp.Items.Clear();

            cmbPotMp.Items.Add("Automatic");

            for (int i = 14; i < 42; i++)
            {
                switch (GetItemId(i))
                {
                    case 389020000: cmbPotMp.Items.Add("1920"); break;
                    case 389019000: cmbPotMp.Items.Add("960"); break;
                    case 389018000: cmbPotMp.Items.Add("480"); break;
                    case 389017000: cmbPotMp.Items.Add("240"); break;
                    case 389016000: cmbPotMp.Items.Add("120"); break;
                }
            }
        }

        private void chkPotHp_CheckedChanged(object sender, EventArgs e)
        {
            timerPotion.Enabled = chkPotHp.Checked || chkPotMp.Checked;
        }

        private void chkPotMp_CheckedChanged(object sender, EventArgs e)
        {
            timerPotion.Enabled = chkPotHp.Checked || chkPotMp.Checked;
        }

        private void chkSupplyEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSupplyEnable.Checked && GetZoneId() != KarusEslant)
            {
                chkSupplyEnable.Checked = false;
                MessageBox.Show("Karakter uygun bolgede degil.");
                return;
            }

            foreach (Control cont in groupSupply.Controls)
            {
                if (cont is CheckBox && cont != chkSupplyEnable)
                {
                    ((CheckBox)cont).Enabled = chkSupplyEnable.Checked;
                }
                else if (cont is NumericUpDown)
                {
                    ((NumericUpDown)cont).Enabled = chkSupplyEnable.Checked;
                }
            }

            timerSupplyControl.Enabled = chkSupplyEnable.Checked;
        }

        private void chkEscape_CheckedChanged(object sender, EventArgs e)
        {
            timerEscape.Enabled = chkEscape.Checked;
        }

        private void btnUpgrade_Click(object sender, EventArgs e)
        {
            UpgradeItems();
        }

        private void GetFindUpgradeSC()
        {
            cmbUpgradeSc.Items.Clear();

            string UpgradeSCName = "";

            List<int> UpgradeSCList = new()
            {
                379221000,
                379205000,
                379016000,
                379021000,
                379022000,
                379025000,
                379030000,
                379031000,
                379032000,
                379033000
            };

            for (int i = 14; i < 42; i++)
            {
                if (UpgradeSCList.Contains(GetItemId(i)))
                {
                    switch (GetItemId(i))
                    {
                        case 379221000: UpgradeSCName = "Upgrade Scroll (Low Class item)"; break;
                        case 379205000: UpgradeSCName = "Upgrade Scroll (Middle Class item)"; break;
                        case 379016000: UpgradeSCName = "Upgrade Scroll"; break;
                        case 379021000: UpgradeSCName = "Blessed Upgrade Scroll"; break;
                        case 379025000: UpgradeSCName = "Blessed Elemental Scroll"; break;
                        case 379022000: UpgradeSCName = "Blessed Enchant Scroll(STR)"; break;
                        case 379030000: UpgradeSCName = "Blessed Enchant Scroll(HP)"; break;
                        case 379031000: UpgradeSCName = "Blessed Enchant Scroll(DEX)"; break;
                        case 379032000: UpgradeSCName = "Blessed Enchant Scroll(INT)"; break;
                        case 379033000: UpgradeSCName = "Blessed Enchant Scroll(MAGIC)"; break;
                    }
                    cmbUpgradeSc.Items.Add(UpgradeSCName);
                }
            }
        }

        public void UpgradeItems()
        {
            Thread UpgradeItemThread = new(() =>
            {
                int UpgradeSCID = 0;
                int ItemID, ItemSlot;
                Random random = new();

                switch (cmbUpgradeSc.Text)
                {
                    case "Upgrade Scroll (Low Class item)": UpgradeSCID = 379221000; break;
                    case "Upgrade Scroll (Middle Class item)": UpgradeSCID = 379205000; break;
                    case "Upgrade Scroll": UpgradeSCID = 379016000; break;
                    case "Blessed Upgrade Scroll": UpgradeSCID = 379021000; break;
                    case "Blessed Elemental Scroll": UpgradeSCID = 379025000; break;
                    case "Blessed Enchant Scroll(STR)": UpgradeSCID = 379022000; break;
                    case "Blessed Enchant Scroll(HP)": UpgradeSCID = 379030000; break;
                    case "Blessed Enchant Scroll(DEX)": UpgradeSCID = 379031000; break;
                    case "Blessed Enchant Scroll(INT)": UpgradeSCID = 379032000; break;
                    case "Blessed Enchant Scroll(MAGIC)": UpgradeSCID = 379033000; break;
                }

                int UpgradeSCSlot = GetInventoryItemSlot(UpgradeSCID);
                int UpgradeSCCount = CheckItemCount(UpgradeSCSlot);

                if (UpgradeSCCount == 0 || cmbUpgradeSc.Text == null)
                {
                    MessageBox.Show("Yukseltme parsomeni envanterde degil veya yukseltme parsomeni secili degil.");
                    return;
                }

                if (txtUpgMaxDelay.Value <= txtUpgMinDelay.Value && txtUpgMaxDelay.Enabled)
                    txtUpgMaxDelay.Value += 1;

                for (int i = 14; i < 42; i++)
                {
                    ItemID = GetItemId(i);
                    ItemSlot = GetInventoryItemSlotFast(ItemID, i);

                    if (ItemSlot != -1 && ItemSlot != UpgradeSCSlot && ItemID > 100000000 && ItemID < 370000000)
                    {
                        lblUpgStatus.ForeColor = Color.Black;
                        lblUpgStatus.Text = "Devam ediyor...";

                        SendPacket("5B02" + "01" + "CCAF" + "0000" + AlignDWORD(ItemID) + AlignDWORD(ItemSlot - 14)[..2] +
                            AlignDWORD(UpgradeSCID) + AlignDWORD(UpgradeSCSlot - 14)[..2] +
                            "00000000FF00000000FF00000000FF00000000FF00000000FF00000000FF00000000FF00000000FF");

                        UpgradeSCCount -= 1;

                        if (UpgradeSCCount == 0)
                            break;

                        if (!chkRandom.Checked)
                        {
                            Thread.Sleep((int)txtUpgMinDelay.Value);
                            lblUpgDelay.Text = $"Delay: {txtUpgMinDelay.Value}";
                        }
                        else
                        {
                            int randomNum = random.Next((int)txtUpgMinDelay.Value, (int)txtUpgMaxDelay.Value);
                            Thread.Sleep(randomNum);
                            lblUpgDelay.Text = $"Delay: {randomNum}";
                        }
                    }
                }

                lblUpgStatus.ForeColor = Color.Red; lblUpgStatus.Text = "Bitti";

            })
            {
                IsBackground = true
            };
            UpgradeItemThread.Start();
        }

        private void cmbUpgradeSc_DropDown(object sender, EventArgs e)
        {
            GetFindUpgradeSC();
        }

        private void chkRandom_CheckedChanged(object sender, EventArgs e)
        {
            lblBetween.Enabled = chkRandom.Checked;
            txtUpgMaxDelay.Enabled = chkRandom.Checked;
        }

        private void chkMinorHeal_CheckedChanged(object sender, EventArgs e)
        {
            timerMinor.Enabled = chkMinorHeal.Checked;
        }

        private void timerParty_Tick(object sender, EventArgs e)
        {
            if (chkPartySend.Checked)
            {
                if (timeAfterPartyRequest == null)
                    return;

                for (int i = 0; i < lstPlayers.Items.Count; i++)
                {
                    if (lstPlayers.GetItemChecked(i))
                    {
                        if (!IsPartyMember(timeAfterPartyRequest[i].Nick) && timeAfterPartyRequest[i].Second >= 13)
                        {
                            SendParty(timeAfterPartyRequest[i].Nick);

                            lstPartyInfo.Items.Add(timeAfterPartyRequest[i].Nick + " | " + DateTime.Now.ToString("HH:mm:ss"));

                            lstPartyInfo.TopIndex = lstPartyInfo.Items.Count - 1;

                            timeAfterPartyRequest[i].Second = 0;
                        }
                        timeAfterPartyRequest[i].Second += 1;
                    }
                }
            }

            if (chkAutoParty.Checked && /*GetPartyCount() == 0 &&*/ IsThereAPartyReq())
                SendPacket("2F0201");

            if (chkPartyControl.Checked && GetPartyCount() == 0)
            {
                Disconnect();
                txtStatus.Text = "Party bozuldugu icin oyun kapatildi";
                txtStatus.ForeColor = Color.Red;
            }

            lstPartyPlayers.Items.Clear();

            for (int i = 0; i <= GetPartyCount() - 1; i++)
                lstPartyPlayers.Items.Add(GetPartyName(i));

            if (lstPartyInfo.Items.Count > 100)
                lstPartyInfo.Items.Clear();
        }

        private void btnAddNearbyPlayers_Click(object sender, EventArgs e)
        {
            List<TargetInfo> PlayerList = new List<TargetInfo>();

            if (SearchPlayer(ref PlayerList) > 0)
            {
                PlayerList.ForEach(x =>
                {
                    if (x.Base > 0 && x.Name != null && !lstPlayers.Items.Contains(x.Name))
                    {
                        lstPlayers.Items.Add(x.Name);
                    }
                });
            }

            for (int i = 0; i < lstPlayers.Items.Count; i++)
            {
                if (timeAfterPartyRequest == null)
                    continue;

                timeAfterPartyRequest.Add(new PRequestList { Nick = $"{lstPlayers.Items[i]}", Second = 13 });
            }
        }

        private void btnAddPlayerNick_Click(object sender, EventArgs e)
        {
            if (!txtNick.Text.Equals("") && !lstPlayers.Items.Contains(txtNick.Text))
                lstPlayers.Items.Add(txtNick.Text);

            if (timeAfterPartyRequest == null)
                return;

            timeAfterPartyRequest.Add(new PRequestList { Nick = txtNick.Text, Second = 13 });
        }

        private void btnClearPlayerList_Click(object sender, EventArgs e)
        {
            lstPlayers.Items.Clear();

            if (timeAfterPartyRequest == null)
                return;

            timeAfterPartyRequest.Clear();
        }

        private void btnDeletePlayer_Click(object sender, EventArgs e)
        {
            if (timeAfterPartyRequest == null)
                return;

            timeAfterPartyRequest.RemoveAt(lstPlayers.SelectedIndex);

            if (lstPlayers.SelectedItems.Count > 0)
                lstPlayers.Items.RemoveAt(lstPlayers.SelectedIndex);
        }

        private void chkPartySend_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPartySend.Checked || chkAutoParty.Checked || chkPartyControl.Checked)
                timerParty.Enabled = true;
            else
                timerParty.Enabled = false;
        }

        private void chkAutoParty_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPartySend.Checked || chkAutoParty.Checked || chkPartyControl.Checked)
                timerParty.Enabled = true;
            else
                timerParty.Enabled = false;
        }

        private void chkPartyControl_CheckedChanged(object sender, EventArgs e)
        {
            if (GetPartyCount() == 0 && chkPartyControl.Checked)
            {
                chkPartyControl.Checked = false;
                MessageBox.Show("Partiye girdikten sonra aktiflestirin.");
                return;
            }

            if (chkPartySend.Checked || chkAutoParty.Checked || chkPartyControl.Checked)
                timerParty.Enabled = true;
            else
                timerParty.Enabled = false;
        }

        private void txtDelay_ValueChanged(object sender, EventArgs e)
        {
            if (txtDelay.Value < 1250 && Job == "Rogue" || txtDelay.Value < 1250 && Job == "Mage")
                txtDelay.ForeColor = Color.Red;
            else
                txtDelay.ForeColor = Color.Black;

            Delay = (int)txtDelay.Value;
        }

        private void chkRunToTarget_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRunToTarget.Checked && chkRunToTarget.ForeColor != Color.Red && lstSkills.CheckedItems.Contains("Super Archer"))
                chkRunToTarget.ForeColor = Color.Red;
            else
                chkRunToTarget.ForeColor = Color.Black;

        }

        //private void Form1_Load(object sender, EventArgs e)
        //{
        //    TableHandler.Load().ContinueWith((task) =>
        //    {
        //        applicationReady = true;
        //    });
        //}
    }
}
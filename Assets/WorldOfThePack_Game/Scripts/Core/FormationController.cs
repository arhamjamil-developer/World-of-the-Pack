using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public enum FormationRole
    {
        Tank,       // Front row, absorbs damage (Hero Wolvy, Captain Bobo)
        Skirmisher, // Middle row, rapid damage (Scout Pip)
        Ranger,     // Middle row, ranged damage (Princess Elva, Tinker Tilly)
        Support,    // Back row, healing & buffs (Fairy Lulu)
        Mage        // Back row, area spell damage (Wizard Momo)
    }

    [System.Serializable]
    public class FormationSlot
    {
        public int slotIndex;
        public string preferredHeroId;
        public FormationRole role;
        public Vector2 localOffset;
        public HeroUnit assignedHero;

        public bool IsOccupied => assignedHero != null && assignedHero.IsAlive;
    }

    public class FormationController : MonoBehaviour
    {
        public static FormationController Instance { get; private set; }

        [Header("Formation Anchor Movement")]
        [SerializeField] private float marchSpeed = 1.1f;
        [SerializeField] private float engagedSpeed = 0.2f;
        [SerializeField] private bool isMarching = false;
        [SerializeField] private Vector3 anchorPosition = new Vector3(0f, -1.4f, 0f);

        [Header("Formation Slots (3 Rows)")]
        [SerializeField] private List<FormationSlot> slots = new List<FormationSlot>();

        public Vector3 AnchorPosition => anchorPosition;
        public bool IsMarching => isMarching;
        public float CurrentSpeed => isMarching ? (HasEngagedAllies() ? engagedSpeed : marchSpeed) : 0f;

        public event Action<FormationSlot, HeroUnit> OnHeroAssignedToSlot;
        public event Action<FormationSlot> OnSlotVacated;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            InitializeFormationSlots();
        }

        private void Update()
        {
            if (isMarching)
            {
                float speed = CurrentSpeed;
                anchorPosition += Vector3.up * (speed * Time.deltaTime);
                transform.position = anchorPosition;
            }
        }

        public void SetMarching(bool marching)
        {
            isMarching = marching;
        }

        private int currentFormationIndex = 0;
        public int CurrentFormationIndex => currentFormationIndex;

        public void CycleFormation()
        {
            currentFormationIndex = (currentFormationIndex + 1) % 3;
            ApplyFormationPattern(currentFormationIndex);
        }

        private void ApplyFormationPattern(int patternIndex)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var s = slots[i];
                switch (patternIndex)
                {
                    case 0: // Standard Wedge (Mockup Reference)
                        s.localOffset = s.preferredHeroId switch
                        {
                            "HeroWolvy" => new Vector2(-0.42f, 0.65f),
                            "CaptainBobo" => new Vector2(0.45f, 0.65f),
                            "PrincessElva" => new Vector2(-0.68f, 0.02f),
                            "WizardMomo" => new Vector2(0.0f, -0.15f),
                            "ScoutPip" => new Vector2(0.92f, 0.18f),
                            "FairyLulu" => new Vector2(-0.75f, -0.78f),
                            "TinkerTilly" => new Vector2(0.78f, -0.75f),
                            _ => s.localOffset
                        };
                        break;
                    case 1: // Defensive Wall
                        s.localOffset = s.preferredHeroId switch
                        {
                            "HeroWolvy" => new Vector2(0.0f, 0.5f),
                            "CaptainBobo" => new Vector2(-0.85f, 0.5f),
                            "PrincessElva" => new Vector2(0.85f, 0.5f),
                            "WizardMomo" => new Vector2(0.0f, -0.4f),
                            "ScoutPip" => new Vector2(-1.1f, -0.3f),
                            "FairyLulu" => new Vector2(0.6f, -0.6f),
                            "TinkerTilly" => new Vector2(-0.6f, -0.6f),
                            _ => s.localOffset
                        };
                        break;
                    case 2: // Dual Column
                        s.localOffset = s.preferredHeroId switch
                        {
                            "HeroWolvy" => new Vector2(-0.55f, 0.7f),
                            "CaptainBobo" => new Vector2(0.55f, 0.7f),
                            "PrincessElva" => new Vector2(-0.55f, 0.0f),
                            "WizardMomo" => new Vector2(0.55f, 0.0f),
                            "ScoutPip" => new Vector2(-0.55f, -0.7f),
                            "FairyLulu" => new Vector2(0.55f, -0.7f),
                            "TinkerTilly" => new Vector2(0.0f, -1.1f),
                            _ => s.localOffset
                        };
                        break;
                }
            }
        }

        public void SetAnchorPosition(Vector3 pos)
        {
            anchorPosition = pos;
            transform.position = anchorPosition;
        }

        private void InitializeFormationSlots()
        {
            slots.Clear();

            // FRONT ROW: Tanks & Melee (Leading the pack)
            // Slot 0: Hero Wolvy (Center-left lead)
            slots.Add(new FormationSlot
            {
                slotIndex = 0,
                preferredHeroId = "HeroWolvy",
                role = FormationRole.Tank,
                localOffset = new Vector2(-0.42f, 0.65f)
            });

            // Slot 1: Captain Bobo (Center-right lead)
            slots.Add(new FormationSlot
            {
                slotIndex = 1,
                preferredHeroId = "CaptainBobo",
                role = FormationRole.Tank,
                localOffset = new Vector2(0.45f, 0.65f)
            });

            // MIDDLE ROW: Skirmishers & Rangers
            // Slot 2: Princess Elva (Mid-left archer)
            slots.Add(new FormationSlot
            {
                slotIndex = 2,
                preferredHeroId = "PrincessElva",
                role = FormationRole.Ranger,
                localOffset = new Vector2(-0.68f, 0.02f)
            });

            // Slot 3: Wizard Momo (Center magical controller)
            slots.Add(new FormationSlot
            {
                slotIndex = 3,
                preferredHeroId = "WizardMomo",
                role = FormationRole.Mage,
                localOffset = new Vector2(0.0f, -0.15f)
            });

            // Slot 4: Scout Pip (Mid-right skirmisher)
            slots.Add(new FormationSlot
            {
                slotIndex = 4,
                preferredHeroId = "ScoutPip",
                role = FormationRole.Skirmisher,
                localOffset = new Vector2(0.92f, 0.18f)
            });

            // BACK ROW: Support & Engineering
            // Slot 5: Fairy Lulu (Rear-left healer)
            slots.Add(new FormationSlot
            {
                slotIndex = 5,
                preferredHeroId = "FairyLulu",
                role = FormationRole.Support,
                localOffset = new Vector2(-0.75f, -0.78f)
            });

            // Slot 6: Tinker Tilly (Rear-right engineer)
            slots.Add(new FormationSlot
            {
                slotIndex = 6,
                preferredHeroId = "TinkerTilly",
                role = FormationRole.Ranger,
                localOffset = new Vector2(0.78f, -0.75f)
            });
        }

        /// <summary>
        /// Assigns a hero to their preferred slot, or the best available open slot matching role.
        /// </summary>
        public FormationSlot AssignHero(HeroUnit hero)
        {
            if (hero == null) return null;

            // 1. Try preferred slot
            FormationSlot targetSlot = slots.Find(s => s.preferredHeroId == hero.HeroId && !s.IsOccupied);

            // 2. Fallback to any empty slot
            if (targetSlot == null)
            {
                targetSlot = slots.Find(s => !s.IsOccupied);
            }

            if (targetSlot != null)
            {
                targetSlot.assignedHero = hero;
                hero.OnDefeated += OnAssignedHeroDefeated;
                OnHeroAssignedToSlot?.Invoke(targetSlot, hero);
                return targetSlot;
            }

            return null;
        }

        public void ReleaseHero(HeroUnit hero)
        {
            if (hero == null) return;
            FormationSlot slot = slots.Find(s => s.assignedHero == hero);
            if (slot != null)
            {
                slot.assignedHero = null;
                hero.OnDefeated -= OnAssignedHeroDefeated;
                OnSlotVacated?.Invoke(slot);
            }
        }

        private void OnAssignedHeroDefeated(Combatant c)
        {
            if (c is HeroUnit hu)
            {
                ReleaseHero(hu);
            }
        }

        public Vector3 GetSlotWorldPosition(HeroUnit hero)
        {
            if (hero == null) return anchorPosition;

            // Prioritize the assigned formation slot for clean, beautiful spacing matching the mockup
            FormationSlot slot = slots.Find(s => s.assignedHero == hero);
            if (slot != null)
            {
                return anchorPosition + new Vector3(slot.localOffset.x, slot.localOffset.y, 0f);
            }

            if (hero.IsAlpha)
            {
                return anchorPosition + new Vector3(LaneSystem.GetLaneX(2), hero.FormationYOffset, 0f);
            }

            if (hero.AssignedLaneIndex >= 0)
            {
                float laneX = LaneSystem.GetLaneX(hero.AssignedLaneIndex);
                float yOff = hero.FormationYOffset;
                return anchorPosition + new Vector3(laneX, yOff, 0f);
            }

            return anchorPosition;
        }

        public int GetPreferredLaneForHero(string heroId)
        {
            return heroId switch
            {
                "HeroWolvy" => 2,   // Center (Alpha)
                "CaptainBobo" => 1, // Mid Left Tank
                "PrincessElva" => 3, // Mid Right Archer
                "ScoutPip" => 0,    // Flank Left Skirmisher
                "TinkerTilly" => 4, // Flank Right Ranger
                "FairyLulu" => 2,   // Center Back Support
                "WizardMomo" => 3,  // Mid Right Back Mage
                _ => 2
            };
        }

        public Vector3 GetSlotWorldPositionByIndex(int index)
        {
            if (index >= 0 && index < slots.Count)
            {
                return anchorPosition + new Vector3(slots[index].localOffset.x, slots[index].localOffset.y, 0f);
            }
            return anchorPosition;
        }

        public Vector3 GetPreferredWorldPosition(string heroId)
        {
            FormationSlot slot = slots.Find(s => s.preferredHeroId == heroId);
            if (slot != null)
            {
                return anchorPosition + new Vector3(slot.localOffset.x, slot.localOffset.y, 0f);
            }
            return anchorPosition;
        }

        private bool HasEngagedAllies()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsOccupied && slots[i].assignedHero.IsEngagedInCombat)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(anchorPosition, 0.2f);

            if (slots != null)
            {
                foreach (var s in slots)
                {
                    Vector3 slotPos = anchorPosition + new Vector3(s.localOffset.x, s.localOffset.y, 0f);
                    Gizmos.color = s.IsOccupied ? Color.green : Color.yellow;
                    Gizmos.DrawWireCube(slotPos, new Vector3(0.6f, 0.6f, 0.1f));
                }
            }
        }
    }
}

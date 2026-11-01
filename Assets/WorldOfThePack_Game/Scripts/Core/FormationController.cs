using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
        private static FormationController instance;
        public static FormationController Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<FormationController>();
                return instance;
            }
            private set => instance = value;
        }

        [Header("Formation Anchor & Touch Movement")]
        [Tooltip("Base forward march speed when marching automatically")]
        [SerializeField] private float marchSpeed = 1.1f;
        [Tooltip("Forward speed when player touches or drags forward")]
        [SerializeField] private float touchAdvanceSpeed = 2.5f;
        [Tooltip("Minimum forward crawl speed when not touching")]
        [SerializeField] private float idleCrawlSpeed = 0.35f;
        [Tooltip("Movement speed when any ally is actively engaged in combat")]
        [SerializeField] private float engagedSpeed = 0.25f;
        [Tooltip("Lateral steering speed across corridor")]
        [SerializeField] private float steerSpeed = 3.5f;
        [SerializeField] private bool allowTouchSteering = true;
        [SerializeField] private bool isMarching = false;
        [SerializeField] private Vector3 anchorPosition = new Vector3(0f, 0.4f, 0f);

        [Header("Formation Slots (Leader-First Layout)")]
        [SerializeField] private List<FormationSlot> slots = new List<FormationSlot>();

        private float currentForwardSpeed;
        private float targetAnchorX = 0f;
        private int currentFormationIndex = 0;
        private UnityEngine.EventSystems.PointerEventData uiPointer;
        private readonly List<UnityEngine.EventSystems.RaycastResult> uiHits = new List<UnityEngine.EventSystems.RaycastResult>();

        public Vector3 AnchorPosition => anchorPosition;
        public bool IsMarching => isMarching;
        public float CurrentSpeed => currentForwardSpeed;
        public int CurrentFormationIndex => currentFormationIndex;
        public string FormationName => currentFormationIndex == 1 ? "GUARD" : currentFormationIndex == 2 ? "FOCUS" : "MARCH";

        public event Action<FormationSlot, HeroUnit> OnHeroAssignedToSlot;
        public event Action<FormationSlot> OnSlotVacated;

        private void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this) { Destroy(this); return; }

            InitializeFormationSlots();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Update()
        {
            if (GameDirector.Instance != null && GameDirector.Instance.CurrentState != GameState.Playing) return;
            if (GameDirector.Instance != null && GameDirector.Instance.IsDecisionPending) return;
            if (CampManager.Instance != null && CampManager.Instance.IsAtCamp) return;
            if (isMarching)
            {
                HandleTouchProgression();
            }
        }

        /// <summary>
        /// Player controls forward progression by touch.
        /// Rule: "by touch they can go to further and only can go further".
        /// Anchor Y only advances (+Y), never retreats backward.
        /// </summary>
        private void HandleTouchProgression()
        {
            bool isTouching = false;
            Vector2 touchScreenPos = Vector2.zero;

            // 1. Touchscreen input (Mobile / iOS)
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                isTouching = true;
                touchScreenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            // 2. Mouse input (Unity Editor testing)
            else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                isTouching = true;
                touchScreenPos = Mouse.current.position.ReadValue();
            }

            float speedGoal = idleCrawlSpeed;
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (isTouching && eventSystem != null)
            {
                if (uiPointer == null) uiPointer = new UnityEngine.EventSystems.PointerEventData(eventSystem);
                uiPointer.position = touchScreenPos;
                uiHits.Clear();
                eventSystem.RaycastAll(uiPointer, uiHits);
                if (uiHits.Count > 0) isTouching = false;
            }
            if (isTouching && Camera.main != null && !Camera.main.pixelRect.Contains(touchScreenPos)) isTouching = false;

            if (isTouching && Camera.main != null)
            {
                // Ignore touches on bottom 33% of screen (card UI and hand tray area)
                if (touchScreenPos.y > Screen.height * 0.33f)
                {
                    Vector3 worldTouch = Camera.main.ScreenToWorldPoint(new Vector3(touchScreenPos.x, touchScreenPos.y, 10f));

                    // Touch is ahead of anchor: advance forward!
                    if (worldTouch.y > anchorPosition.y)
                    {
                        float forwardDiff = worldTouch.y - anchorPosition.y;
                        speedGoal = Mathf.Lerp(1.2f, touchAdvanceSpeed, Mathf.Clamp01(forwardDiff / 3.2f));
                    }

                    // Lateral steering within corridor [-1.25, +1.25]
                    if (allowTouchSteering)
                    {
                        targetAnchorX = Mathf.Clamp(worldTouch.x, -1.25f, 1.25f);
                    }
                }
            }

            // Lock/slow progression during intense melee clashes
            if (HasEngagedAllies())
            {
                speedGoal = Mathf.Min(speedGoal, engagedSpeed);
            }

            // Smooth forward progression (strictly forward, Y can never decrease)
            currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, speedGoal, Time.deltaTime * 6f);
            anchorPosition.y += Mathf.Max(0f, currentForwardSpeed) * Time.deltaTime;

            // Smooth lateral steering across lanes
            anchorPosition.x = Mathf.Lerp(anchorPosition.x, targetAnchorX, Time.deltaTime * steerSpeed);

            transform.position = anchorPosition;
        }

        public void SetMarching(bool marching)
        {
            isMarching = marching;
            if (!marching) currentForwardSpeed = 0f;
        }

        public void SetAnchorPosition(Vector3 pos)
        {
            anchorPosition = pos;
            targetAnchorX = pos.x;
            transform.position = anchorPosition;
        }

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
                    case 0: // Standard Wedge (Leader at the tip/top)
                        s.localOffset = s.preferredHeroId switch
                        {
                            "HeroWolvy" => new Vector2(0.0f, 0.65f),      // Leader at top center
                            "CaptainBobo" => new Vector2(-0.55f, 0.15f),  // Beside on left
                            "PrincessElva" => new Vector2(0.55f, 0.15f),  // Beside on right
                            "FairyLulu" => new Vector2(0.0f, -0.40f),     // Rear support
                            "ScoutPip" => new Vector2(-0.85f, -0.25f),    // Left skirmisher
                            "WizardMomo" => new Vector2(0.85f, -0.25f),   // Right mage
                            "TinkerTilly" => new Vector2(0.0f, -0.85f),   // Rear engineer
                            _ => s.localOffset
                        };
                        break;
                    case 1: // Defensive Wall
                        s.localOffset = s.preferredHeroId switch
                        {
                            "HeroWolvy" => new Vector2(0.0f, 0.45f),
                            "CaptainBobo" => new Vector2(-0.70f, 0.45f),
                            "PrincessElva" => new Vector2(0.70f, 0.45f),
                            "WizardMomo" => new Vector2(0.50f, -0.35f),
                            "ScoutPip" => new Vector2(-0.50f, -0.35f),
                            "FairyLulu" => new Vector2(0.0f, -0.65f),
                            "TinkerTilly" => new Vector2(0.0f, -1.05f),
                            _ => s.localOffset
                        };
                        break;
                    case 2: // Column (Corridor Sprint)
                        s.localOffset = s.preferredHeroId switch
                        {
                            "HeroWolvy" => new Vector2(0.0f, 0.70f),
                            "CaptainBobo" => new Vector2(-0.45f, 0.20f),
                            "PrincessElva" => new Vector2(0.45f, 0.20f),
                            "ScoutPip" => new Vector2(-0.45f, -0.30f),
                            "WizardMomo" => new Vector2(0.45f, -0.30f),
                            "FairyLulu" => new Vector2(0.0f, -0.65f),
                            "TinkerTilly" => new Vector2(0.0f, -1.05f),
                            _ => s.localOffset
                        };
                        break;
                }
            }
        }

        public void ResetFormationSlots()
        {
            if (slots == null) return;
            foreach (var s in slots)
            {
                s.assignedHero = null;
            }
            currentFormationIndex = 0;
            currentForwardSpeed = 0;
            InitializeFormationSlots();
        }

        private void InitializeFormationSlots()
        {
            slots.Clear();

            // FRONT: Leader Wolvy at the top center
            slots.Add(new FormationSlot
            {
                slotIndex = 0,
                preferredHeroId = "HeroWolvy",
                role = FormationRole.Tank,
                localOffset = new Vector2(0.0f, 0.65f)
            });

            // MID-LEFT: Captain Bobo beside Wolvy
            slots.Add(new FormationSlot
            {
                slotIndex = 1,
                preferredHeroId = "CaptainBobo",
                role = FormationRole.Tank,
                localOffset = new Vector2(-0.55f, 0.15f)
            });

            // MID-RIGHT: Princess Elva beside Wolvy
            slots.Add(new FormationSlot
            {
                slotIndex = 2,
                preferredHeroId = "PrincessElva",
                role = FormationRole.Ranger,
                localOffset = new Vector2(0.55f, 0.15f)
            });

            // REAR SUPPORT: Fairy Lulu
            slots.Add(new FormationSlot
            {
                slotIndex = 3,
                preferredHeroId = "FairyLulu",
                role = FormationRole.Support,
                localOffset = new Vector2(0.0f, -0.40f)
            });

            // FLANK LEFT: Scout Pip
            slots.Add(new FormationSlot
            {
                slotIndex = 4,
                preferredHeroId = "ScoutPip",
                role = FormationRole.Skirmisher,
                localOffset = new Vector2(-0.85f, -0.25f)
            });

            // FLANK RIGHT: Wizard Momo
            slots.Add(new FormationSlot
            {
                slotIndex = 5,
                preferredHeroId = "WizardMomo",
                role = FormationRole.Mage,
                localOffset = new Vector2(0.85f, -0.25f)
            });

            // REAR ANCHOR: Tinker Tilly
            slots.Add(new FormationSlot
            {
                slotIndex = 6,
                preferredHeroId = "TinkerTilly",
                role = FormationRole.Ranger,
                localOffset = new Vector2(0.0f, -0.85f)
            });
        }

        public FormationSlot AssignHero(HeroUnit hero)
        {
            if (hero == null) return null;

            // 1. Preferred hero slot
            FormationSlot targetSlot = slots.Find(s => s.preferredHeroId == hero.HeroId && !s.IsOccupied);

            // 2. Any vacant slot
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

            FormationSlot slot = slots.Find(s => s.assignedHero == hero);
            if (slot != null)
            {
                return anchorPosition + new Vector3(slot.localOffset.x, slot.localOffset.y, 0f);
            }

            if (hero.IsAlpha)
            {
                return anchorPosition + new Vector3(0f, 0.65f, 0f);
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

        public bool HasEngagedAllies()
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
                    Gizmos.DrawWireCube(slotPos, new Vector3(0.5f, 0.5f, 0.1f));
                }
            }
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WorldOfThePack.NewUI
{
    /// <summary>One stop per three-card window, shared by touch, mouse and dot buttons.</summary>
    public sealed class PackCardPager : ScrollRect
    {
        public Button[] dots;
        public Sprite selectedDot;
        public Sprite idleDot;
        public int stops = 5;
        public int CurrentPage { get; private set; }
        private bool _dragging;
        private float _target;
        private float _startPosition;

        protected override void Start()
        {
            base.Start();
            for (int i = 0; i < dots.Length; i++)
            {
                int page = i;
                dots[i].onClick.AddListener(() => SelectPage(page));
            }
            SelectPage(0);
        }
        public void SelectPage(int page)
        {
            CurrentPage = Mathf.Clamp(page, 0, Mathf.Max(0, stops - 1));
            _target = stops > 1 ? (float)CurrentPage / (stops - 1) : 0;
            velocity = Vector2.zero;
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i].image.sprite = i == CurrentPage ? selectedDot : idleDot;
                dots[i].GetComponent<RectTransform>().localScale = Vector3.one * (i == CurrentPage ? 1.15f : 1f);
            }
        }
        public override void OnBeginDrag(PointerEventData e)
        {
            if (WorldOfThePack.Game.GameDirector.Instance != null &&
                WorldOfThePack.Game.GameDirector.Instance.CurrentState != WorldOfThePack.Game.GameState.Playing) return;
            _dragging = true;
            _startPosition = horizontalNormalizedPosition;
            base.OnBeginDrag(e);
        }
        public override void OnEndDrag(PointerEventData e)
        {
            base.OnEndDrag(e);
            if (!_dragging) return;
            _dragging = false;
            float p = Mathf.Clamp01(horizontalNormalizedPosition);
            int nearest = Mathf.RoundToInt(p * (stops - 1));
            if (nearest == CurrentPage && Mathf.Abs(p - _startPosition) > 0.025f)
                nearest += p > _startPosition ? 1 : -1;
            SelectPage(nearest);
        }
        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (!_dragging && content != null)
                horizontalNormalizedPosition = Mathf.Lerp(horizontalNormalizedPosition, _target, 1 - Mathf.Exp(-18 * Time.unscaledDeltaTime));
        }
    }
}

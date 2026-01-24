using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yesterfriday.GameplayCommonSystems.Cards;

namespace Yesterfriday.GameplayCommonSystems.Cards.Demo
{
    public sealed class CardsDemoController : MonoBehaviour
    {
        [Header("Deck Setup")]
        [Tooltip("CardsModel 생성자에 전달할 스타터 덱. (CardsModel.Reset이 private이므로 여기 리스트가 초기 덱이 됨)")]
        [SerializeField] private List<CardDefinition> starterDeck = new List<CardDefinition>();

        [Header("UI - Counters (TMP)")]
        [SerializeField] private TMP_Text deckCountText;
        [SerializeField] private TMP_Text handCountText;
        [SerializeField] private TMP_Text discardCountText;

        [Header("UI - Log (TMP, Optional)")]
        [Tooltip("선택: 화면에 로그를 보여주고 싶으면 연결. (없으면 Console만 출력)")]
        [SerializeField] private TMP_Text logText;

        [Header("UI - Buttons")]
        [SerializeField] private Button createButton;
        [SerializeField] private Button shuffleButton;
        [SerializeField] private Button peekButton;
        [SerializeField] private Button drawButton;
        [SerializeField] private Button discardButton;
        [SerializeField] private Button playButton;

        [Header("Demo Settings")]
        [SerializeField] private int shuffleSeed = 123;
        [SerializeField] private int peekN = 5;
        [SerializeField] private int drawN = 1;

        private CardsModel _model;

        // TryDraw가 내부에서 Shuffle을 호출하면 이벤트가 2번 올 수 있음.
        // 데모에서 중복 이벤트로 로그/갱신이 과하게 찍히지 않게 간단히 디바운스.
        private bool _pendingRefresh;

        private void Awake()
        {
            WireButtons();
        }

        private void Start()
        {
            // 데모 편의: 시작 시 바로 생성
            CreateOrRecreateModel();
        }

        private void Update()
        {
            // 이벤트가 여러 번 와도 프레임당 1회만 UI 갱신
            if (_pendingRefresh)
            {
                _pendingRefresh = false;
                RefreshCounters();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeModel();
        }

        private void WireButtons()
        {
            if (createButton != null) createButton.onClick.AddListener(CreateOrRecreateModel);
            if (shuffleButton != null) shuffleButton.onClick.AddListener(Shuffle);
            if (peekButton != null) peekButton.onClick.AddListener(Peek);
            if (drawButton != null) drawButton.onClick.AddListener(Draw);
            if (discardButton != null) discardButton.onClick.AddListener(DiscardHand0);
            if (playButton != null) playButton.onClick.AddListener(PlayHand0);
        }

        private void CreateOrRecreateModel()
        {
            UnsubscribeModel();

            // CardsModel은 기본 생성자/Reset(public)이 없으므로 생성자 덱 주입이 필수
            _model = new CardsModel(starterDeck);
            _model.OnCardsChanged += HandleCardsChanged;

            Log($"CreateOrRecreateModel: starterDeck={starterDeck?.Count ?? 0}");
            RefreshCounters();
        }

        private void UnsubscribeModel()
        {
            if (_model == null) return;
            _model.OnCardsChanged -= HandleCardsChanged;
            _model = null;
        }

        private void HandleCardsChanged()
        {
            // 이벤트가 여러 번 와도 UI 갱신을 과도하게 하지 않게 플래그만 세움
            _pendingRefresh = true;
        }

        private void Shuffle()
        {
            if (!EnsureModel()) return;

            bool ok = _model.Shuffle(shuffleSeed);
            Log($"Shuffle(seed={shuffleSeed}) => {ok}");
            RefreshCounters();
        }

        private void Peek()
        {
            if (!EnsureModel()) return;

            var top = _model.Peek(peekN);

            var sb = new StringBuilder();
            sb.Append($"Peek({peekN}) => ");

            if (top.Count == 0)
            {
                sb.Append("(empty)");
            }
            else
            {
                for (int i = 0; i < top.Count; i++)
                {
                    var card = top[i];
                    // CardDefinition에 DisplayName/Id 프로퍼티가 있다고 가정(당신이 권장 리팩토링한 형태)
                    // 만약 public field라면 card.displayName / card.GetId()로 바꿔야 함.
                    sb.Append(card != null ? $"{card.DisplayName}({card.Id})" : "null");
                    if (i < top.Count - 1) sb.Append(", ");
                }
            }

            Log(sb.ToString());
        }

        private void Draw()
        {
            if (!EnsureModel()) return;

            bool ok = _model.TryDraw(drawN, out int drawn, allowReshuffleDiscard: true);
            Log($"TryDraw(requested={drawN}, allowReshuffleDiscard=true) => ok={ok}, drawn={drawn}");
            RefreshCounters();
        }

        private void DiscardHand0()
        {
            if (!EnsureModel()) return;

            bool ok = _model.TryDiscard(0);
            Log($"TryDiscard(handIndex=0) => {ok}");
            RefreshCounters();
        }

        private void PlayHand0()
        {
            if (!EnsureModel()) return;

            bool ok = _model.TryPlay(0);
            Log($"TryPlay(handIndex=0) => {ok} (v0.1 = discard)");
            RefreshCounters();
        }

        private bool EnsureModel()
        {
            if (_model != null) return true;

            Log("Model is null. Click Create first.");
            return false;
        }

        private void RefreshCounters()
        {
            if (_model == null) return;

            int deck = _model.GetCount(CardsZone.Deck);
            int hand = _model.GetCount(CardsZone.Hand);
            int discard = _model.GetCount(CardsZone.Discard);

            if (deckCountText != null) deckCountText.text = $"Deck: {deck}";
            if (handCountText != null) handCountText.text = $"Hand: {hand}";
            if (discardCountText != null) discardCountText.text = $"Discard: {discard}";
        }

        private void Log(string message)
        {
            Debug.Log($"[CardsDemo] {message}");

            if (logText == null) return;

            // 간단한 누적 로그(최근 내용이 아래로)
            logText.text = string.IsNullOrEmpty(logText.text)
                ? message
                : $"{logText.text}\n{message}";
        }
    }
}

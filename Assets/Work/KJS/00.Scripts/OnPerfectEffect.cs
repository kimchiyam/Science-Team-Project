using System;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Work.KJS._00.Scripts
{
    public class OnPerfectEffect : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        [SerializeField] private PlayerController _playerController;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            _playerController.OnPerfect += PerfectEffect;
        }

        public async void PerfectEffect(int combo)
        {
            _text.text = $"Perfect! \nCombo:{combo}";
            _text.rectTransform.DOScale(1.2f, 0.5f).SetEase(Ease.OutQuad);
            _text.rectTransform.DOScale(1f, 0.25f).SetEase(Ease.OutQuad);
            await Task.Delay(1000);
            _text.text = null;

        }
    }
}
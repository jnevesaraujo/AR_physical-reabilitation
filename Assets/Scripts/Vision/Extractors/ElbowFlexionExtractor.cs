using App.Data.ScriptableObjects;
using App.Vision.Evaluators;
using UnityEngine;

namespace App.Vision.Extractors
{
    public class ElbowFlexionExtractor : BaseExerciseExtractor
    {
        private ElbowFlexionEvaluator _evaluator;
        private Transform _shoulder, _elbow, _wrist;
        private ElbowFlexionDefinition _elbowDef;

        protected override void OnInitialize()
        {
            _evaluator = new ElbowFlexionEvaluator(_exerciseDef as ElbowFlexionDefinition);
            _evaluator.OnWarningTriggered += HandleBadPosture;
            _evaluator.OnPostureRestored += HandlePostureRestored;
            _evaluator.OnRepetitionCompleted += HandleRepetitionSuccess;
            _evaluator.OnCalibrationReady += HandleCalibrationReady;
            _evaluator.OnDiscoveryCompleted += HandleDiscoveryCompleted;

            if (_hud != null)
                _hud.OnPeakConfirmRequested += ConfirmPeak;
        }

        protected override void CalibrateAndStart()
        {
            // O Avaliador do Cotovelo exige os 3 pontos para a calibração inicial
            if (!AssignJoints()) return;

            _evaluator.CalibrateBaseline(_shoulder.position, _elbow.position, _wrist.position);
            _isCalibrated = true;
        }

        protected override void OnEvaluateFrame()
        {
            if (!_isCalibrated || !AssignJoints()) return;

            _evaluator.EvaluateFrame(
                _shoulder.position,
                _elbow.position,
                _wrist.position,
                out float currentProgress,
                out bool isDiscovering);

            // Chamada do visualizador correto do cotovelo (adicionado na instrução anterior)
            _visualizer.UpdateElbowVisuals(_shoulder.position, _elbow.position, _wrist.position, currentProgress, isDiscovering);
        }

        protected override void OnSessionComplete()
        {
            Debug.Log("[ElbowFlexion] Session complete.");
            _evaluator.Reset();
        }

        // Elbow-specific handlers
        private void ConfirmPeak()
        {
            _evaluator.ConfirmDiscovery();
            
            // Inicializa a representação gráfica do Goniómetro Virtual na posição do cotovelo
            _visualizer.InitializeGuide(_exerciseDef as ElbowFlexionDefinition, _elbow.position);
        }

        private void HandleCalibrationReady()
        {
            _hud.HideWarning();
            _hud.ShowConfirmPeakButton();
            _hud.ShowWarning("Flicta o braço naturalmente para definir o seu limite e confirme.");
        }

        private void HandleDiscoveryCompleted()
        {
            _hud.HideConfirmPeakButton();
            _hud.HideWarning();
            // A mensagem agora pode desaparecer após 3 segundos ou ficar fixa.
            _hud.ShowWarning("Exercício iniciado. Estique e dobre o cotovelo lentamente."); 
        }

        // Helper para mapear os índices corporais do MediaPipe
        private bool AssignJoints()
        {
            if (_shoulder != null && _elbow != null && _wrist != null) return true;
            if (_pointList == null || _pointList.childCount < 17) return false;

            // Utiliza o indicador "isRightArm" caso esteja definido num Enum ou variável na sua base class, 
            // caso contrário assuma braço direito (12, 14, 16) ou altere conforme o seu modelo.
            bool isRight = true; // Substitua por _elbowDef.isRightArm se adicionou essa propriedade

            _shoulder = _pointList.GetChild(isRight ? 12 : 11);
            _elbow = _pointList.GetChild(isRight ? 14 : 13);
            _wrist = _pointList.GetChild(isRight ? 16 : 15);

            return true;
        }
    }
}
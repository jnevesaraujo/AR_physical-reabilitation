using UnityEngine;
using UnityEngine.UIElements;
using App.Data.Models;

namespace App.UI.Toolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class ProfileView : MonoBehaviour
    {
        private Label _lblUsername;
        private TextField _txtSubjectId, _txtEmail, _txtSurgeryDate;
        private DropdownField _drpAffectedSide;
        private Button _btnSave, _btnChangePass;

        public event System.Action OnSaveRequested;
        public event System.Action OnChangePasswordRequested;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _lblUsername = root.Q<Label>("profile_lbl_username");
            _txtSubjectId = root.Q<TextField>("profile_subject_id");
            _txtEmail = root.Q<TextField>("profile_email");
            _txtSurgeryDate = root.Q<TextField>("profile_surgery_date");
            _drpAffectedSide = root.Q<DropdownField>("profile_affected_side");
            _btnSave = root.Q<Button>("btn_save_profile");
            _btnChangePass = root.Q<Button>("btn_change_password");

            _btnSave?.RegisterCallback<ClickEvent>(OnSaveClicked);
            _btnChangePass?.RegisterCallback<ClickEvent>(OnChangePassClicked);
        }

        private void OnDisable()
        {
            _btnSave?.UnregisterCallback<ClickEvent>(OnSaveClicked);
            _btnChangePass?.UnregisterCallback<ClickEvent>(OnChangePassClicked);
        }

        private void OnSaveClicked(ClickEvent evt) => OnSaveRequested?.Invoke();
        private void OnChangePassClicked(ClickEvent evt) => OnChangePasswordRequested?.Invoke();

        public string SubjectIdInput => _txtSubjectId?.value ?? string.Empty;
        public string EmailInput => _txtEmail?.value ?? string.Empty;
        public string SurgeryDateInput => _txtSurgeryDate?.value ?? string.Empty;
        public AffectedSide AffectedSideInput => MapDropdownToEnum(_drpAffectedSide?.value);

        public void SetUsername(string text)
        {
            if (_lblUsername != null) _lblUsername.text = text;
        }

        public void SetEmail(string text)
        {
            if (_txtEmail != null) _txtEmail.value = text;
        }

        public void SetSubjectId(string text)
        {
            if (_txtSubjectId != null) _txtSubjectId.value = text;
        }

        public void SetSurgeryDate(string text)
        {
            if (_txtSurgeryDate != null) _txtSurgeryDate.value = text;
        }

        public void SetAffectedSide(AffectedSide side)
        {
            if (_drpAffectedSide != null) _drpAffectedSide.value = MapEnumToDropdown(side);
        }

        public void SetSaveButtonState(bool enabled, string text)
        {
            if (_btnSave == null) return;
            _btnSave.SetEnabled(enabled);
            _btnSave.text = text;
        }

        private AffectedSide MapDropdownToEnum(string dropdownValue)
        {
            if (string.IsNullOrEmpty(dropdownValue)) return AffectedSide.Unknown;
            string val = dropdownValue.ToLower();
            if (val.Contains("esquerd")) return AffectedSide.Left;
            if (val.Contains("direit")) return AffectedSide.Right;
            if (val.Contains("bilateral") || val.Contains("ambos")) return AffectedSide.Bilateral;
            return AffectedSide.Unknown;
        }

        private string MapEnumToDropdown(AffectedSide side) => side switch
        {
            AffectedSide.Left => "Esquerdo",
            AffectedSide.Right => "Direito",
            AffectedSide.Bilateral => "Bilateral",
            _ => "Indefinido"
        };
    }
}
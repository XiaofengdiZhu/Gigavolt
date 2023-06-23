namespace Game {
    public abstract class BaseEditGVMemoryBankDialog : Dialog {
        public virtual GVArrayData GetArrayData() => null;
        public virtual void UpdateFromData() { }
        public virtual void Dismiss(bool result, bool hide = true) { }
    }
}
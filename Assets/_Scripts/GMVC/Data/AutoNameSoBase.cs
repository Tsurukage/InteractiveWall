using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GMVC.Data
{
    /// <summary>
    /// 自动文件名SO
    /// </summary>
    public class AutoUnderscoreNamingObject : AutoNameWithSeparatorSoBase
    {
        const char Underscore = '_';
        protected override char Separator => Underscore;
    }

    public class AutoDashNamingObject : AutoNameWithSeparatorSoBase
    {
        const char Dash = '-';
        protected override char Separator => Dash;
    }

    public class AutoBacktickNamingObject : AutoNameWithSeparatorSoBase
    {
        const char Backtick = '`';
        protected override char Separator => Backtick;
    }

    public class AutoHashNamingObject : AutoNameWithSeparatorSoBase
    {
        const char Hash = '#';
        protected override char Separator => Hash;
    }

    public class AutoAtNamingObject : AutoNameWithSeparatorSoBase
    {
        const char At = '@';
        protected override char Separator => At;
    }

    public abstract class AutoNameWithSeparatorSoBase : AutoNameSoBase, IDataElement
    {
        public virtual int Id => id;
        [SerializeField] protected int id;

        protected abstract char Separator { get; }
        protected override string GetName() => string.Join(Separator, id, base.GetName());
    }

    public abstract class AutoNameSoBase : ScriptableObject
    {
#if UNITY_EDITOR
        [ReadOnly,SerializeField,OnInspectorInit(nameof(GetReference))] ScriptableObject referenceSo;
        protected bool GetReference()
        {
            if (referenceSo == null) referenceSo = this;
            ChangeName();
            return false;

            void ChangeName()
            {
                var path = AssetDatabase.GetAssetPath(this);
                var newName = GetName();
                if (string.IsNullOrWhiteSpace(newName)) return;
                var err = AssetDatabase.RenameAsset(path, newName);
                if (!string.IsNullOrWhiteSpace(err)) Debug.LogError(err);
            }
        }
#endif
        protected virtual string Prefix { get; } = string.Empty;
        protected virtual string Suffix { get; } = string.Empty;
#if UNITY_EDITOR
        [OnValueChanged(nameof(GetReference))] 
#endif
        [SerializeField] protected string _name;
        public virtual string Name => _name;
        protected virtual string GetName() => (Prefix ?? "") + Name + (Suffix ?? "");
    }

    public abstract class ReferenceSoBase : ScriptableObject
    {
#if UNITY_EDITOR
        [ReadOnly,SerializeField,OnInspectorInit(nameof(GetReference))] ScriptableObject referenceSo;
        protected void GetReference()
        {
            if (referenceSo == null) referenceSo = this;
        }
#endif
    }
}
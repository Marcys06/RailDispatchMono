using RailDispatchMono.Core.Effects;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RailDispatchMono.Core.Settings
{
    public class RailDispatchMonoSettings : INotifyPropertyChanged
    {
        private bool fullScreen;
        private int language = 2;
        private ParticleEffectType particleEffect;
        private int windowWidth = 1600;
        private int windowHeight = 900;

        public bool FullScreen { get => fullScreen; set { if (fullScreen != value) { fullScreen = value; OnPropertyChanged(); } } }
        public int Language { get => language; set { if (language != value) { language = value; OnPropertyChanged(); } } }
        public ParticleEffectType ParticleEffect { get => particleEffect; set { if (particleEffect != value) { particleEffect = value; OnPropertyChanged(); } } }
        public int WindowWidth { get => windowWidth; set { if (windowWidth != value) { windowWidth = value; OnPropertyChanged(); } } }
        public int WindowHeight { get => windowHeight; set { if (windowHeight != value) { windowHeight = value; OnPropertyChanged(); } } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;

namespace WindowsMicToggle.Services
{
    public class MicService
    {
        private List<MMDevice> _devices = new List<MMDevice>();

        public MicService()
        {
            InitializeMicrophone();
        }

        public bool IsMuted { get; private set; }

        public event EventHandler<bool> MicrophoneStateChanged;

        private void InitializeMicrophone()
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            
            var collection = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.All);
            
            foreach (MMDevice device in collection)
            {
                _devices.Add(device);
            
            }

            IsMuted = false;

            SetMicState();
        }

        public void ToggleMicrophone()
        {
            try
            {
                IsMuted = !IsMuted;
                SetMicState();
                MicrophoneStateChanged?.Invoke(this, IsMuted);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to toggle microphone", ex);
            }
        }

        private void SetMicState()
        {
            foreach (var device in _devices)
            {
                try
                {
                    device.AudioEndpointVolume.Mute = IsMuted;
                }
                catch {  }
            }
        }
    }
}

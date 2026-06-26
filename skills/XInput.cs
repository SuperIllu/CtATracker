using CtATracker.config;
using System.Runtime.InteropServices;

namespace CtATracker.skills
{
    internal static class XInput
    {
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_DEVICE_NOT_CONNECTED = 1167;

        public const ushort XINPUT_GAMEPAD_DPAD_UP = 0x0001;
        public const ushort XINPUT_GAMEPAD_DPAD_DOWN = 0x0002;
        public const ushort XINPUT_GAMEPAD_DPAD_LEFT = 0x0004;
        public const ushort XINPUT_GAMEPAD_DPAD_RIGHT = 0x0008;
        public const ushort XINPUT_GAMEPAD_START = 0x0010;
        public const ushort XINPUT_GAMEPAD_BACK = 0x0020;
        public const ushort XINPUT_GAMEPAD_LEFT_THUMB = 0x0040;
        public const ushort XINPUT_GAMEPAD_RIGHT_THUMB = 0x0080;
        public const ushort XINPUT_GAMEPAD_LEFT_SHOULDER = 0x0100;
        public const ushort XINPUT_GAMEPAD_RIGHT_SHOULDER = 0x0200;
        public const ushort XINPUT_GAMEPAD_A = 0x1000;
        public const ushort XINPUT_GAMEPAD_B = 0x2000;
        public const ushort XINPUT_GAMEPAD_X = 0x4000;
        public const ushort XINPUT_GAMEPAD_Y = 0x8000;

        public static byte TRIGGER_THRESHOLD => ConfigLoader.Instance.Gamepad.TriggerThreshold;

        [DllImport("xinput1_4.dll")]
        public static extern int XInputGetState(int dwUserIndex, ref XINPUT_STATE pState);

        public static int GetState(out XINPUT_STATE state)
        {
            state = new XINPUT_STATE();
            return XInputGetState(0, ref state);
        }

        public static GamepadButton? DetectButtonPress(XINPUT_STATE current, XINPUT_STATE previous)
        {
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_A)) return GamepadButton.A;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_B)) return GamepadButton.B;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_X)) return GamepadButton.X;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_Y)) return GamepadButton.Y;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_LEFT_SHOULDER)) return GamepadButton.LeftShoulder;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_RIGHT_SHOULDER)) return GamepadButton.RightShoulder;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_START)) return GamepadButton.Start;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_BACK)) return GamepadButton.Back;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_LEFT_THUMB)) return GamepadButton.LeftStick;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_RIGHT_THUMB)) return GamepadButton.RightStick;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_DPAD_UP)) return GamepadButton.DPadUp;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_DPAD_DOWN)) return GamepadButton.DPadDown;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_DPAD_LEFT)) return GamepadButton.DPadLeft;
            if (IsFlagJustPressed(current, previous, XINPUT_GAMEPAD_DPAD_RIGHT)) return GamepadButton.DPadRight;
            if (current.Gamepad.bLeftTrigger > TRIGGER_THRESHOLD && previous.Gamepad.bLeftTrigger <= TRIGGER_THRESHOLD)
                return GamepadButton.LeftTrigger;
            if (current.Gamepad.bRightTrigger > TRIGGER_THRESHOLD && previous.Gamepad.bRightTrigger <= TRIGGER_THRESHOLD)
                return GamepadButton.RightTrigger;
            return null;
        }

        public static bool IsButtonHeld(XINPUT_STATE state, GamepadButton button)
        {
            return button switch
            {
                GamepadButton.A => (state.Gamepad.wButtons & XINPUT_GAMEPAD_A) != 0,
                GamepadButton.B => (state.Gamepad.wButtons & XINPUT_GAMEPAD_B) != 0,
                GamepadButton.X => (state.Gamepad.wButtons & XINPUT_GAMEPAD_X) != 0,
                GamepadButton.Y => (state.Gamepad.wButtons & XINPUT_GAMEPAD_Y) != 0,
                GamepadButton.LeftShoulder => (state.Gamepad.wButtons & XINPUT_GAMEPAD_LEFT_SHOULDER) != 0,
                GamepadButton.RightShoulder => (state.Gamepad.wButtons & XINPUT_GAMEPAD_RIGHT_SHOULDER) != 0,
                GamepadButton.Start => (state.Gamepad.wButtons & XINPUT_GAMEPAD_START) != 0,
                GamepadButton.Back => (state.Gamepad.wButtons & XINPUT_GAMEPAD_BACK) != 0,
                GamepadButton.LeftStick => (state.Gamepad.wButtons & XINPUT_GAMEPAD_LEFT_THUMB) != 0,
                GamepadButton.RightStick => (state.Gamepad.wButtons & XINPUT_GAMEPAD_RIGHT_THUMB) != 0,
                GamepadButton.DPadUp => (state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_UP) != 0,
                GamepadButton.DPadDown => (state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_DOWN) != 0,
                GamepadButton.DPadLeft => (state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_LEFT) != 0,
                GamepadButton.DPadRight => (state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_RIGHT) != 0,
                GamepadButton.LeftTrigger => state.Gamepad.bLeftTrigger > TRIGGER_THRESHOLD,
                GamepadButton.RightTrigger => state.Gamepad.bRightTrigger > TRIGGER_THRESHOLD,
                _ => false
            };
        }

        private static bool IsFlagJustPressed(XINPUT_STATE current, XINPUT_STATE previous, ushort flag)
        {
            return (current.Gamepad.wButtons & flag) != 0 && (previous.Gamepad.wButtons & flag) == 0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XINPUT_STATE
    {
        public uint dwPacketNumber;
        public XINPUT_GAMEPAD Gamepad;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XINPUT_GAMEPAD
    {
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }
}

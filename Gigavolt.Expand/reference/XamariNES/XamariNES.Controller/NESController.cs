using XamariNES.Common.Extensions;
using XamariNES.Controller.Enums;

namespace XamariNES.Controller {
    /// <summary>
    ///     Standard NES Controller
    ///     https://wiki.nesdev.com/w/index.php/Controller_reading
    ///     https://wiki.nesdev.com/w/index.php/Standard_controller
    /// </summary>
    public class NESController : IController {
        byte _buttonStatusShift;
        bool _isPolling;

        public byte ButtonStates { get; set; }

        /// <summary>
        ///     Sets the flag for the given button as pressed
        /// </summary>
        /// <param name="button"></param>
        public void ButtonPress(enumButtons button) {
            ButtonStates |= (byte)button;
        }

        /// <summary>
        ///     Sets the flag for the given button as released
        /// </summary>
        /// <param name="button"></param>
        public void ButtonRelease(enumButtons button) {
            ButtonStates &= (byte)~button;
        }

        /// <summary>
        ///     The CPU signals the controller that it's going to be polling
        ///     for button status, or that it is done polling
        /// </summary>
        /// <param name="input"></param>
        public void SignalController(byte input) {
            if (input.IsBitSet(0)) {
                _isPolling = false;
                _buttonStatusShift = 0;
            }
            else {
                _isPolling = true;
            }
        }

        /// <summary>
        ///     Poll the controller for the next button state in the sequence
        /// </summary>
        /// <returns></returns>
        public byte ReadController() {
            //Non-Standard NES remotes support values beyond 7 bits, for those
            //we'll just return 1 for now.
            if (_buttonStatusShift > 7) {
                return 1;
            }
            byte buttonState = (byte)(ButtonStates.IsBitSet(_buttonStatusShift) ? 1 : 0);
            if (_isPolling) {
                _buttonStatusShift++;
            }
            return buttonState;
        }
    }
}
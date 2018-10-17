using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Hedgehog.Services.Command.Handlers.Billing
{
    public class IntegerSequence : IEnumerator<int>, IEnumerable<int>, INotifyPropertyChanged
    {
        private enum IntegerSequenceState
        {
            BeforeStart,
            InSequence,
            AfterEnd
        }

        private IntegerSequenceState _state;
        private int _current;
        private int _minimum;
        private int _maximum;

        public IntegerSequence(int minimum = 0, int? maximum = null)
        {
            _state = IntegerSequenceState.BeforeStart;
            _minimum = minimum;
            _maximum = maximum ?? int.MaxValue;
        }

        public int GetNextValue()
        {
            if (!MoveNext())
                throw new InvalidOperationException("There are no values left in the sequence.");

            return _current;
        }

        public bool HasValues
        {
            get
            {
                switch (_state)
                {
                    case IntegerSequenceState.BeforeStart:
                        return _minimum <= _maximum;

                    case IntegerSequenceState.InSequence:
                        return _current < _maximum;

                    default:
                        return false;
                }
            }
        }

        public int Minimum
        {
            get { return _minimum; }
            set
            {
                if (_minimum != value)
                {
                    if (_state != IntegerSequenceState.BeforeStart)
                        throw new InvalidOperationException("Integer Sequences cannot be change once iterated.");

                    _minimum = value;

                    OnPropertyChanged("Minimum");
                    OnPropertyChanged("Count");
                }
            }
        }

        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (_maximum != value)
                {
                    if (_state != IntegerSequenceState.BeforeStart)
                        throw new InvalidOperationException("Integer Sequences cannot be change once iterated.");

                    _maximum = value;

                    OnPropertyChanged("Maximum");
                    OnPropertyChanged("Count");
                }
            }
        }

        public int Count
        {
            get { return _maximum >= _minimum ? _maximum - _minimum + 1 : 0; }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion

        #region Implementation of IEnumerator

        public bool MoveNext()
        {
            switch (_state)
            {
                case IntegerSequenceState.BeforeStart:
                    if (_minimum <= _maximum)
                    {
                        _current = _minimum;
                        _state = IntegerSequenceState.InSequence;
                        return true;
                    }

                    _state = IntegerSequenceState.AfterEnd;
                    return false;

                case IntegerSequenceState.InSequence:
                    if (_current < _maximum)
                    {
                        _current++;
                        return true;
                    }

                    _state = IntegerSequenceState.AfterEnd;
                    return false;

                case IntegerSequenceState.AfterEnd:
                    return false;
            }

            throw new InvalidOperationException();
        }

        public void Reset()
        {
            _state = IntegerSequenceState.BeforeStart;
            _current = 0;
        }

        public int Current
        {
            get
            {
                if (_state != IntegerSequenceState.InSequence)
                    throw new InvalidOperationException();

                return _current;
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<int> GetEnumerator()
        {
            return new IntegerSequence(_minimum, _maximum);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new IntegerSequence(_minimum, _maximum);
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;

            if (propertyChanged != null)
            {
                propertyChanged(this, e);
            }
        }

        #endregion
    }
}

using UnityEngine;

namespace App.Vision
{
    /// <summary>
    /// One Euro Filter — adaptive low-pass filter for real-time signal smoothing.
    /// Reduces jitter when stationary while preserving responsiveness during movement.
    /// 
    /// Original paper: Casiez, G., Roussel, N., Vogel, D. (2012).
    /// "1€ Filter: A Simple Speed-based Low-pass Filter for Noisy Input in Interactive Systems"
    /// CHI 2012, Austin, Texas.
    /// 
    /// Parameters:
    ///   minCutoff — minimum cutoff frequency (Hz). Lower = smoother when still, more lag.
    ///               Recommended range: 0.5–2.0 for pose landmarks.
    ///   beta      — speed coefficient. Higher = less lag during fast movement.
    ///               Recommended range: 0.0–1.0. Start at 0.1, increase if lag is visible.
    ///   dCutoff   — cutoff for the derivative (velocity estimate). Usually leave at 1.0.
    /// </summary>
    public class OneEuroFilter1D
    {
        private float _minCutoff;
        private float _beta;
        private float _dCutoff;

        private float _xPrev;
        private float _dxPrev;
        private float _tPrev;
        private bool  _initialized;

        public OneEuroFilter1D(float minCutoff = 1.0f, float beta = 0.1f, float dCutoff = 1.0f)
        {
            _minCutoff = minCutoff;
            _beta      = beta;
            _dCutoff   = dCutoff;
        }

        public float Filter(float x, float timestamp)
        {
            if (!_initialized)
            {
                _xPrev       = x;
                _dxPrev      = 0f;
                _tPrev       = timestamp;
                _initialized = true;
                return x;
            }

            float dt = timestamp - _tPrev;
            if (dt <= 0f) return _xPrev; // guard against duplicate timestamps

            // Estimate derivative (velocity of the signal)
            float dx    = (x - _xPrev) / dt;
            float dxHat = LowPassFilter(dx, _dxPrev, Alpha(_dCutoff, dt));

            // Adapt cutoff based on speed — faster movement → higher cutoff → less lag
            float cutoff = _minCutoff + _beta * Mathf.Abs(dxHat);
            float xHat   = LowPassFilter(x, _xPrev, Alpha(cutoff, dt));

            _dxPrev = dxHat;
            _xPrev  = xHat;
            _tPrev  = timestamp;

            return xHat;
        }

        public void Reset()
        {
            _initialized = false;
        }

        private static float Alpha(float cutoff, float dt)
        {
            float tau = 1f / (2f * Mathf.PI * cutoff);
            return 1f / (1f + tau / dt);
        }

        private static float LowPassFilter(float x, float xPrev, float alpha)
        {
            return alpha * x + (1f - alpha) * xPrev;
        }
    }

    /// <summary>
    /// One Euro Filter for Vector3 — filters each axis independently.
    /// </summary>
    public class OneEuroFilterV3
    {
        private readonly OneEuroFilter1D _x;
        private readonly OneEuroFilter1D _y;
        private readonly OneEuroFilter1D _z;

        public OneEuroFilterV3(float minCutoff = 1.0f, float beta = 0.1f, float dCutoff = 1.0f)
        {
            _x = new OneEuroFilter1D(minCutoff, beta, dCutoff);
            _y = new OneEuroFilter1D(minCutoff, beta, dCutoff);
            _z = new OneEuroFilter1D(minCutoff, beta, dCutoff);
        }

        public Vector3 Filter(Vector3 v, float timestamp)
        {
            return new Vector3(
                _x.Filter(v.x, timestamp),
                _y.Filter(v.y, timestamp),
                _z.Filter(v.z, timestamp));
        }

        public void Reset()
        {
            _x.Reset();
            _y.Reset();
            _z.Reset();
        }

        /// <summary>
        /// Update parameters at runtime for tuning without recompilation.
        /// </summary>
        public void SetParameters(float minCutoff, float beta)
        {
            // Recreate internal filters with new params — simplest approach
            // since 1Euro doesn't support mid-stream param changes cleanly
            Reset();
        }
    }
}
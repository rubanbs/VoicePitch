namespace WebApi.Core;

/// <summary>
/// Простейший детектор высоты тона (Pitch / F0)
/// на основе автокорреляции.
/// Работает с одним аудио-фреймом (float[] PCM).
/// </summary>
public class PitchDetector
{
    // Частота дискретизации аудио (например 44100 Гц)
    private readonly int _sampleRate;

    // Минимальная и максимальная задержка (lag),
    // соответствующие диапазону частот голоса
    private readonly int _minLag;
    private readonly int _maxLag;

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="sampleRate">Частота дискретизации (например 44100)</param>
    /// <param name="minFreq">Минимальная частота голоса (обычно 80 Гц)</param>
    /// <param name="maxFreq">Максимальная частота голоса (обычно 400 Гц)</param>
    public PitchDetector(int sampleRate, float minFreq = 80f, float maxFreq = 400f)
    {
        _sampleRate = sampleRate;

        // Переводим диапазон частот в диапазон задержек (lag)
        // Формула: lag = sampleRate / frequency
        _minLag = (int)(sampleRate / maxFreq);
        _maxLag = (int)(sampleRate / minFreq);
    }

    /// <summary>
    /// Определяет высоту тона (pitch) в переданном аудио-фрейме.
    /// </summary>
    /// <param name="buffer">Массив PCM-сэмплов (float[])</param>
    /// <returns>Частота в Гц или 0 если не определена</returns>
    public float DetectPitch(float[] buffer)
    {
        int size = buffer.Length;

        // Удаление DC-смещения.
        // Иногда сигнал имеет смещение вверх или вниз.
        // Это может исказить автокорреляцию.
        // Вычитаем среднее значение.

        float mean = 0f;
        for (int i = 0; i < size; i++)
            mean += buffer[i];

        mean /= size;

        for (int i = 0; i < size; i++)
            buffer[i] -= mean;

        // Проверка энергии сигнала.
        // Если сигнал слишком тихий (тишина),
        // нет смысла искать pitch.

        float energy = 0f;
        for (int i = 0; i < size; i++)
            energy += buffer[i] * buffer[i];

        if (energy < 0.001f)
            return 0f;

        // Автокорреляция.
        // Ищем задержку (lag), при которой
        // сигнал максимально похож сам на себя.
        // Это и будет период колебаний.
        float maxCorrelation = 0f;
        int bestLag = 0;

        for (int lag = _minLag; lag <= _maxLag; lag++)
        {
            float correlation = 0f;

            for (int i = 0; i < size - lag; i++)
            {
                correlation += buffer[i] * buffer[i + lag];
            }

            // Ищем максимум корреляции
            if (correlation > maxCorrelation)
            {
                maxCorrelation = correlation;
                bestLag = lag;
            }
        }

        // Если лаг не найден
        if (bestLag == 0)
            return 0f;

        // Перевод периода в частоту.
        // Формула:
        // frequency = sampleRate / period

        float frequency = (float)_sampleRate / bestLag;

        return frequency;
    }
}
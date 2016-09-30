using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nn.classes
{
    /// <summary>
    /// Класс, описывающий слой полносвязной нейросети.
    /// Слой характеризуется числом входов,
    /// нейронов и матрицей весов.
    /// Параметры задаются.
    /// </summary>
    class NeuralLayer
    {
        #region fields
        /// <summary>
        /// генератор синапсов для начальной инициализации сети
        /// создаю статическим, чтоб для всех слоев работал один генератор        
        /// </summary>
        private static Random randomGenerator = new Random();
        /// <summary>
        /// число входов и выходов слоя нейросети
        /// </summary>
        private int outputsCount, inputsCount;        
        /// <summary>
        /// матрица весов, [число входов * число нейронов]
        /// </summary>
        private double[][] weights;
        /// <summary>
        /// состояние выходов на текущий момент
        /// </summary>
        private double[] lastOut;
        /// <summary>
        /// Накопленное на входах в данный момент
        /// </summary>
        private double[] lastIn;
        /// <summary>
        /// Храним производные по выходам, входам и весам
        /// </summary>
        private double[] dEdY;
        private double[] dEdX;
        private double[][] dEdW;
        private const int Bias =1;
        #endregion
        #region methods
        /// <summary>
        /// Инициализирует новый слой нейросети (веса не генерятся)
        /// </summary>
        /// <param name="inputsCount">число входов</param>
        /// <param name="outputsCount">число выходов</param>
        public NeuralLayer( int inputsCount,int outputsCount)
        {
            this.outputsCount = outputsCount;
            this.inputsCount = inputsCount;
            dEdY = new double[outputsCount];
            dEdX = new double[outputsCount];
            dEdW = new double[inputsCount + Bias][];
            for (int i = 0; i < outputsCount; i++)
            {
                dEdW[i] = new double[outputsCount]; 
            }
        }
        /// <summary>
        /// Генерирует случайные входные веса
        /// </summary>
        public void GenerateWeights()
        {
            //выделим память под веса
            weights = new double[inputsCount+Bias][];
            //всю матрицу
            for (int i = 0; i < inputsCount+Bias; i++)
            {
                weights[i] = new double[outputsCount];

                for (int j = 0; j < outputsCount; j++)
                    //заполним случайными числами
                    weights[i][j] = randomGenerator.NextDouble()-0.5;
            }
        }
        /// <summary>
        /// Для последнего слоя
        /// </summary>
        /// <param name="idealOut">Идеальный выходной вектор</param>
        public void CountOutDetivativesAsLast(double[] idealOut)
        {
            dEdY = CudaHelper.Subtract(lastOut, idealOut);
            //for (int i = 0; i < outputsCount; i++)
            //{
            //    //простейшая формула производной от ошибки
            //    dEdY[i] = lastOut[i] - idealOut[i];//=dE/dYk3
            //}
        }
        /// <summary>
        /// Считает производные по выходам промежуточного слоя
        /// </summary>
        /// <param name="dEdXPrev">Производные по входам слоя, следующего за данным
        /// (того, в который входят связи, исходящие из данного слоя)</param>
        /// <param name="wPrev">Веса связей, исходящих из данного слоя
        /// (берем набор связей, входящих в следующий слой)</param>
        public void CountOutDerivatives(double[] dEdXPrev,double[][] wPrev)
        {
            //по каждому выходу
            for (int n = 0; n < outputsCount; n++)
            {
                //производная есть сумма, т.к. E=E(x1(y1,y2,...,yn),...,xk(y1,y2,...,yn))
                //k - число входов след. слоя
                //n - число выходов текущего слоя
                //т.о. на каждой веточке вносится вклад в ошибку ~ весу
                double dEdYn = CudaHelper.Multiply(dEdXPrev,wPrev[n]).Sum();                
                dEdY[n] = dEdYn;
            }
        }
        /// <summary>
        /// Считает производные по входам слоя
        /// </summary>
        public void CountInDerivatives()
        {
            //проходя через слой, получаем X->Y, Y=Sg(X)
            //производная сигмоида выражается через его значение
            //for (int i = 0; i < outputsCount; i++)
            //{
            //    double dYdX = Sigmoid.betta * lastOut[i] * (1 - lastOut[i]);
            //    dEdX[i] = dEdY[i] * dYdX;
            //}
            double[] dYdX = CudaHelper.Multiply(lastOut, Sigmoid.betta);
            double[] secondProduction = CudaHelper.Subtract(1,lastOut);
            CudaHelper.MultiplyInPlace(ref dYdX, secondProduction);
            dEdX = CudaHelper.Multiply(dEdY, dYdX);
        }
        /// <summary>
        /// Считает производные по весам
        /// </summary>
        /// <param name="yprev">Выход предыдущего слоя</param>
        public void CountWeightDerivatives(double[] yprev)
        {
            //yprev.Lenght = inputsCount
            //dEdX.Lenght = outputsCount
            //Вход для каждого нейрона - линейная комбинация
            //выходов предыдущего (к-т равен весу). Т.о. производная по ij весу
            //(для i входа j нейрона) равна i компоненте входного в-ра данного слоя
            //т.е. выходного в-ра предыдущего
            for (int i = 0; i < inputsCount+Bias; i++)
            {                
                for (int j = 0; j < outputsCount; j++)
                {
                    dEdW[i][j] = dEdX[j] * yprev[i];                 
                }                 
            }
        }
        /// <summary>
        /// Обновляет веса на основании расчитанных производных
        /// </summary>
        /// <param name="delta"></param>
        public void Update(double delta)
        {
            for (int i = 0; i < inputsCount + Bias; i++)
            {
                double[] gradient = CudaHelper.Multiply(dEdW[i], delta);
                CudaHelper.SubtractInPlace(ref weights[i], gradient);                
            }
        }
        #endregion
        #region properties
        /// <summary>
        /// Возвращает или устанавливает число входов
        /// </summary>
        public int InputsCount { get { return inputsCount; } set { inputsCount = value; } }
        /// <summary>
        /// Возвращает или устанивливает число выходов
        /// </summary>
        public int OutputsCount { get { return outputsCount; } set { outputsCount = value; } }
        /// <summary>
        /// Возвращает или устанавливает вес
        /// </summary>
        /// <param name="inp">Номер нейрона в слое</param>
        /// <param name="outp">Номер входа</param>
        /// <returns></returns>
        public double[] this[int inp]
        { 
            get { return weights[inp]; } 
            set { weights[inp] = value; } 
        }
        /// <summary>
        /// Возвращает или устанавливает состояние всех нейронов слоя
        /// </summary>
        public double[] LastOut { get { return lastOut; } set { lastOut = value; } }
        /// <summary>
        /// Возвращает или устанавливает состояние всех входных сигналов
        /// </summary>
        public double[] LastIn { get { return lastIn; } set { lastIn = value; } }
        public double[] DEDX { get { return dEdX; } }
        public double[][] W { get { return weights; } }
        #endregion
    }
}

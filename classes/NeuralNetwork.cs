﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Visn = System.Windows.Forms.DataVisualization;

namespace nn.classes
{
    class Sigmoid
    {
        #region static members
        public static readonly double betta = 1.0;
        /// <summary>
        /// сигмоидальная функция нейрона
        /// </summary>
        /// <param name="input">Вход</param>
        /// <returns>Значение Sg(x)</returns>
        public static double Sg(double input)
        {            
            return (1 / (1 + Math.Exp(-betta * input)));
        }
        #endregion
    }
    /// <summary>
    /// Класс, описывающий нейронную сеть,
    /// которая может состоять из множества слоев.
    /// Число входов, выходов и размеры слоев задаются.
    /// </summary>
    class NeuralNetwork
    {
        #region fields
        /// <summary>
        /// массив слоев сети
        /// </summary>
        private NeuralLayer[] layers;               
        /// <summary>
        /// число входов, выходов, слоев
        /// </summary>
        private int IN=0, OUT=0, layersCount=0;
        private double delta;
        #endregion

        #region methods
        /// <summary>
        /// Генерирует полносвязную многослойную 
        /// нейросеть
        /// </summary>
        /// <param name="inputs">Число входных сигналов</param>
        /// <param name="layersSizes">Набор параметров переменной длины, 
        /// содержит размеры слоев сети</param>
        public NeuralNetwork(int inputs, int[] layersSizes)
        {
            //число слоев определяется длиной массива с их размерами
            layersCount = layersSizes.Length;
            //выделим память под слои
            layers = new NeuralLayer[layersCount];
            //текущее число входных сигналов. 
            //сначала равно числу входов всей сети
            int currentInputs = inputs;
            //создадим слои сети
            for (int i = 0; i < layersCount; i++)
            {
                //создадим слой с числом входов currentInputs
                //и числом нейронов (и выходов) равным его размеру
                layers[i] = new NeuralLayer(currentInputs, layersSizes[i]);
                //сгенерируем синапсы (весовые коэффициенты)
                //случайным образом
                layers[i].GenerateWeights();
                //для следующего слоя размер текущего
                //станет числом входов
                currentInputs = layersSizes[i];
            }
            //число входов известно
            IN = inputs;
            //число выходов = размер последнего слоя
            OUT = currentInputs;
        }
        /// <summary>
        /// Расчитывает выходной вектор для нейросети
        /// </summary>
        /// <param name="inX">Вектор входный сигналов</param>
        /// <param name="outY">Вектор выхода</param>
        public void NetworkOut(double[] inX, out double[] outY) 
        {
            double[] input = inX;   
            //по всем слоям сети
            for (int i = 0; i < layersCount; i++)
            {
                double[] output;
                double[] currentInput;
                output = new double[layers[i].OutputsCount+1];
                currentInput = new double[layers[i].OutputsCount];
                //по всем нейронам в слое
                for (int j = 0; j < layers[i].OutputsCount; j++)
                {
                    //расчитаем входное значение
                    double arg = 0.0;
                    //по всем входам нейрона
                    for (int k = 0; k < layers[i].InputsCount+1; k++)
                    {
                        //на каждом шаге надо добавить очередной "вклад"
                        //для j нейрона в i слое по k входу:
                        arg += input[k] * layers[i][k][j];                        
                    }
                    currentInput[j] = arg;
                    //выход нейрона = сигмоидальная функция от входного значения
                    output[j] = Sigmoid.Sg(currentInput[j]);
                }
                output[layers[i].OutputsCount] = 1;
                //сохраним состояние слоя
                layers[i].LastOut = output;
                layers[i].LastIn = currentInput;
                //выход текущего слоя становится входом для следующего
                input = output;
            }
            //когда прошли через выходной слой, в input попал как раз
            //его выходной вектор возбуждения
            outY = input;
        }
        /// <summary>
        /// Расчет ошибки сети
        /// </summary>
        /// <param name="realVector">Вектор возбуждения выходных нейронов</param>
        /// <param name="ideal">"Идеальный" вектор - желаемый ответ сети</param>
        /// <returns></returns>
        private double E(double[] realVector, double[] ideal)
        {
            double[] eVector = CudaHelper.Subtract(realVector, ideal);
            CudaHelper.SqrInPlace(ref eVector);
            return eVector.Sum() / 2;
            //double Err = 0.0;
            //for (int i = 0; i < realVector.Length; i++)
            //    Err += (realVector[i] - ideal[i]) * (realVector[i] - ideal[i]);
            //return Err / 2;
        }
        /// <summary>
        /// Настройка весов под вектора входа и желаемого выхода
        /// </summary>
        /// <param name="ideal"></param>
        /// <param name="inX"></param>
        private void CorrectError(double[] ideal, double[] inX)
        {
            //надо пройти по всем слоям и посчитать производные
            for (int i = layersCount - 1; i >= 0; i--)
            {
                //Если слой выходной, считаем ошибку по выходу 
                //из желаемого вектора
                if (i == layersCount - 1)
                    layers[i].CountOutDetivativesAsLast(ideal);
                //для промежуточных слоев - используем исходящие веса и
                //производные по входам для исходящих связей
                else
                    layers[i].CountOutDerivatives(layers[i + 1].DEDX, layers[i + 1].W);  
                //перепрыгиваем с выхода на вход
                layers[i].CountInDerivatives();
                //посчитаем производные по слою
                //если это входной слой, то для него входным вектором служит inX
                if (i == 0)
                    layers[i].CountWeightDerivatives(inX);
                //Если слой промежуточный, то на его входе "висит" 
                //выход предыдущего
                else
                    layers[i].CountWeightDerivatives(layers[i - 1].LastOut);                
            }
            //Все dE/dW посчитаны, обновим веса
            for (int i = layersCount - 1; i >= 0; i--)
                layers[i].Update(delta);
        }
        /// <summary>
        /// Функция обучения нейросети
        /// </summary>
        /// <param name="sample">Обучающая выборка</param>
        /// <param name="ideals">Идеальные вектора</param>
        /// <param name="Epochs">Число итераций</param>
        /// <param name="e0">Порог допустимой ошибки</param>
        /// <param name="sampleSize">Размер выборки</param>
        /// <returns>"Successfull" - если за Epochs итераций ошибка стала меньше порогового значения
        /// "Unsuccessfull" - если по прошествии Epochs итераций ошибка все еще больше пороговой</returns>
        public String Train(double[][] sample, double[][] ideals, int Epochs, double e0, 
            int sampleSize, Visn.Charting.Chart chart, int displayStep)
        {
            chart.Series[0].Points.Clear();// = new Visn.Charting.Series();
            //накопленная ошибка итерации
            double EpochError=0;
            //номер текущей итерации
            int epoch;
            //цикл по всем итерациям
            for (epoch = 0; epoch < Epochs; epoch++)
            {
                //обнулить накопленную ошибку
                EpochError = .0;
                //по всем векторам выборки
                for (int i = 0; i < sampleSize; i++)
                {
                    //возьмем текущий вектор выборки
                    //и идеальный для него выходной вектор
                    double[] Sample = sample[i];
                    double[] Ideal = ideals[i];
                    double[] nwOut;
                    //пропустим вектор из выборки через сеть
                    this.NetworkOut(Sample, out nwOut);
                    //расчитаем ошибку сети для данного элемента выборки
                    EpochError += E(nwOut, Ideal);
                    //скорректируем веса методом backpropagation
                    CorrectError(Ideal,Sample);
                }
                if (epoch % displayStep == 0)
                {
                    chart.Series[0].Points.AddXY(epoch, EpochError);
                    chart.Invalidate(chart.ClientRectangle);
                    chart.Update();
                }
                //если после просмотра всей выборки ошибка меньше минимальной
                //можно уже сейчас прекратить обучение
                if (EpochError < e0) break;
            }
            //проверим, по какому условию завершился цикл обучения
            if (epoch < Epochs) return "Successfull";
            if (EpochError > e0) return "Unsuccessfull";
            return "Finished";
        }
        #endregion
        public double Delta { get { return delta; } set { delta = value; } }
    }
}

﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Etg.SimpleStubs;
using ServerMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerMonitor.ViewModels;
using ServerMonitor.Models;
using System.Collections.ObjectModel;
using Telerik.UI.Xaml.Controls.Chart;

namespace TestServerMonitor.TestViewModel
{
    [TestClass]
    public class TestChartViewModel
    {
        private ChartPageViewModel viewModel;

        public ChartPalette DefaultPalette { get { return ChartPalettes.DefaultLight; } }
        public List<Site> Sites { get; set; }
        public List<Log> Logs { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            viewModel = new ChartPageViewModel();
            Sites = new List<Site>();
            Logs = new List<Log>();
            for (int i = 1; i <= 5; i++)
            {
                Logs.Add(new Log() { Site_id = i, Is_error = true });
                Sites.Add(new Site() { Id = i, Site_name = "Site" + i, Is_server = true });
            }
        }

        /// <summary>
        /// 测试ChartAsync方法
        /// 用例说明：测试方法正常调用，返回true，返回正确值
        /// </summary>
        [TestMethod]
        public void TestChartAsync_ShouldReturnTrueWhenNormallyExcuted()
        {
            var stub = new StubIChartDao(MockBehavior.Strict);
            viewModel.ChartDao = stub;
            int except1 = 0, except2 = 0, except3 = 0;
            stub.ChartLengendAsync(async (sites) =>
            {
                ObservableCollection<ChartLengend> s = new ObservableCollection<ChartLengend>();
                await Task.CompletedTask;
                except1 = sites.Count;
                return s;
            }, Times.Once);
            stub.CacuChartAsync(async (sites, logs) => 
            {
                ObservableCollection<ObservableCollection<Chart1>> data = new ObservableCollection<ObservableCollection<Chart1>>();
                except2 = sites.Count+logs.Count;
                int[,] array = new int[sites.Count,3];
                await Task.CompletedTask;
                return new Tuple<ObservableCollection<ObservableCollection<Chart1>>, int[,]>(data, array);
            }, Times.Once);
            stub.CacuBarChart((sites, array) =>
            {
                ObservableCollection<BarChartData> data1 = new ObservableCollection<BarChartData>();
                ObservableCollection<BarChartData> data2 = new ObservableCollection<BarChartData>();
                except3 = sites.Count;
                return new Tuple<ObservableCollection<BarChartData>, ObservableCollection<BarChartData>>(data1, data2);
            }, Times.Once);

            Assert.IsTrue(viewModel.ChartAsync(Sites, Logs).Result);
            Assert.AreEqual(5, except1);
            Assert.AreEqual(10, except2);
            Assert.AreEqual(5, except3);
        }

        /// <summary>
        /// 测试Accept_ClickAsync方法
        /// 用例说明：测试选择站点数目小于等于5，返回true,大于5，返回false
        /// </summary>
        [TestMethod]
        public void TestAccept_ClickAsync_ShouldReturnTrueWhenNumberOfSitesLE5()
        {
            Assert.IsTrue(viewModel.InitAsync().Result);
            var stub = new StubIChartDao(MockBehavior.Strict);
            viewModel.ChartDao = stub;
            for (int i = 0; i < 4; i++)
            {
                viewModel.Infos.SelectSites.Add(new SelectSite() { IsSelected = true });
            }
            stub.ChartLengendAsync(async (sites) =>
            {
                ObservableCollection<ChartLengend> s = new ObservableCollection<ChartLengend>();
                await Task.CompletedTask;
                return s;
            }, Times.Once);
            stub.CacuChartAsync(async (sites, logs) =>
            {
                ObservableCollection<ObservableCollection<Chart1>> data = new ObservableCollection<ObservableCollection<Chart1>>();
                int[,] array = new int[sites.Count, 3];
                await Task.CompletedTask;
                return new Tuple<ObservableCollection<ObservableCollection<Chart1>>, int[,]>(data, array);
            }, Times.Twice);
            stub.CacuBarChart((sites, array) =>
            {
                ObservableCollection<BarChartData> data1 = new ObservableCollection<BarChartData>();
                ObservableCollection<BarChartData> data2 = new ObservableCollection<BarChartData>();
                return new Tuple<ObservableCollection<BarChartData>, ObservableCollection<BarChartData>>(data1, data2);
            }, Times.Once);

            //less
            Assert.IsTrue(viewModel.Accept_ClickAsync().Result);
            //equal
            viewModel.Infos.SelectSites.Add(new SelectSite() { IsSelected = true });
            Assert.IsTrue(viewModel.Accept_ClickAsync().Result);

            //greater
            viewModel.Infos.SelectSites.Add(new SelectSite() { IsSelected = true });
            Assert.IsFalse(viewModel.Accept_ClickAsync().Result);
        }

        /// <summary>
        /// 测试TypeChanged_Data方法
        /// 用例说明：测试方法正确计算,返回true
        /// </summary>
        [TestMethod]
        public void TestTypeChanged_ShouldReturnTrueWhenCalculationCorrect()
        {
            Assert.IsTrue(viewModel.InitAsync().Result);
            
            ObservableCollection<Chart1> item = new ObservableCollection<Chart1>();
            item.Add(new Chart1() { Result = "Error" });
            item.Add(new Chart1() { Result = "Success" });
            item.Add(new Chart1() { Result = "OverTime" });
            item.Add(new Chart1() { Result = "Success" });
            item.Add(new Chart1() { Result = "Error" });
            viewModel.Chart1Collection.Add(item);

            Assert.IsTrue(viewModel.TypeChanged("All"));
            Assert.AreEqual(5, viewModel.Infos.Chart1CollectionCopy[0].Count);

            Assert.IsTrue(viewModel.TypeChanged("Success"));
            Assert.AreEqual(2, viewModel.Infos.Chart1CollectionCopy[0].Count);

            Assert.IsTrue(viewModel.TypeChanged("Error"));
            Assert.AreEqual(2, viewModel.Infos.Chart1CollectionCopy[0].Count);

            Assert.IsTrue(viewModel.TypeChanged("OverTime"));
            Assert.AreEqual(1, viewModel.Infos.Chart1CollectionCopy[0].Count);
        }


    }
}
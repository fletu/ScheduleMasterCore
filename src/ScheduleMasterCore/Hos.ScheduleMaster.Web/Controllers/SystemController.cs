﻿using Hos.ScheduleMaster.Core;
using Hos.ScheduleMaster.Core.Common;
using Hos.ScheduleMaster.Core.Interface;
using Hos.ScheduleMaster.Core.Models;
using Hos.ScheduleMaster.Web.Extension;
using Hos.ScheduleMaster.Web.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hos.ScheduleMaster.Web.Controllers
{
    public class SystemController : AdminController
    {
        [Autowired]
        public ISystemService _systemService { get; set; }

        [Autowired]
        public IScheduleService _scheduleService { get; set; }

        // GET: System
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 节点列表页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Node()
        {
            return View();
        }

        /// <summary>
        /// 节点分页数据
        /// </summary>
        /// <returns></returns>
        public ActionResult QueryNodePager(string keyword)
        {
            var pager = new ListPager<ServerNodeEntity>(PageIndex, PageSize);
            if (!string.IsNullOrEmpty(keyword))
            {
                pager.AddFilter(x => x.MachineName.Contains(keyword) || x.NodeName.Contains(keyword) || x.Host.Contains(keyword));
            }
            pager = _systemService.QueryNodePager(pager);
            return GridData(pager.Total, pager.Rows);
        }

        /// <summary>
        /// 节点启用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, AjaxRequestOnly]
        public ActionResult NodeEnable(string name)
        {
            var result = _systemService.NodeSwich(name, 2);
            if (result)
            {
                return this.JsonNet(true, "操作成功！");
            }
            return this.JsonNet(false, "操作失败！");
        }

        /// <summary>
        /// 节点停机
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, AjaxRequestOnly]
        public ActionResult NodeDisable(string name)
        {
            var result = _systemService.NodeSwich(name, 1);
            if (result)
            {
                return this.JsonNet(true, "操作成功！");
            }
            return this.JsonNet(false, "操作失败！");
        }

        /// <summary>
        /// 参数配置页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Config()
        {
            var data = _systemService.GetConfigList();
            return View(data);
        }

        /// <summary>
        /// 保存参数配置
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveConfig(IFormCollection form)
        {
            Dictionary<string, string> items = new Dictionary<string, string>();
            foreach (string key in form.Keys)
            {
                items.Add(key, form[key]);
            }
            bool result = _systemService.SaveConfig(items, CurrentAdmin.UserName);
            if (result)
            {
                ConfigurationCache.Reload();
                return SuccessTip("保存成功！");
            }
            return DangerTip("保存失败！");
        }

        /// <summary>
        /// 日志列表页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Log()
        {
            return View();
        }

        /// <summary>
        /// 查询日志记录
        /// </summary>
        /// <param name="enddate"></param>
        /// <param name="sid"></param>
        /// <param name="startdate"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult QueryLogPager(DateTime? startdate, DateTime? enddate, Guid? sid, int? category)
        {
            var pager = new ListPager<SystemLogEntity>(PageIndex, PageSize);
            if (sid.HasValue)
            {
                pager.AddFilter(m => m.ScheduleId == sid);
            }
            if (category.HasValue && category.Value > 0)
            {
                pager.AddFilter(m => m.Category == category);
            }
            if (startdate.HasValue)
            {
                pager.AddFilter(m => m.CreateTime >= startdate);
            }
            if (enddate.HasValue)
            {
                pager.AddFilter(m => m.CreateTime <= enddate);
            }
            pager = _systemService.QueryLogPager(pager);
            return GridData(pager.Total, pager.Rows);
        }

        /// <summary>
        /// 清理日志页面
        /// </summary>
        /// <returns></returns>
        public ActionResult ClearLog()
        {
            List<SelectListItem> selectData = new List<SelectListItem>();
            selectData.Add(new SelectListItem() { Text = "系统日志", Value = "0" });
            selectData.AddRange(_scheduleService.QueryAll().Select(row => new SelectListItem
            {
                Text = row.Title,
                Value = row.Id.ToString(),
                Selected = false
            }));
            ViewBag.TaskList = selectData;
            return View();
        }


        /// <summary>
        /// 清理日志
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="category"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        [HttpPost, AjaxRequestOnly]
        public ActionResult ClearLog(Guid? sid, int? category, DateTime? startdate, DateTime? enddate)
        {
            var result = _systemService.DeleteLog(sid, category, startdate, enddate);
            if (result > 0)
            {
                return this.JsonNet(true, $"清理成功！本次清理【{result}】条");
            }
            return this.JsonNet(false, "没有符合条件的记录！");
        }
    }
}

//     class Program
//     {
//         static void Main(string[] args)
//         {
//             string server, sysid, uid, pass, caDllName;
//             if (args.Length == 4 || args.Length == 5)
//             {
//                 server = args[0];
//                 sysid = args[1];
//                 uid = args[2];
//                 pass = args[3];
//                 caDllName = args.Length == 5 ? args[4] : null;
//             }
//             else
//             {
//                 Console.WriteLine("{0} server sysid uid pass caDllName", (new System.IO.FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)).Name);
//                 return;
//             }

//             Console.WriteLine("請等待連線完成後且測試新單發送後，收到委託回報後再按 Enter 鍵繼續刪單測試作業");

//             Client client = new Client(caDllName);
//             client.OnApReady += OnApReady;
//             client.OnOrderReport += OnOrderReport;
//             // 連接交易主機
//             client.Connect(server, sysid, uid, pass);
//             Console.ReadLine();
//             // 刪除最新一筆可刪委託
//             SorOrd ord = client.OrdsList.LastOrDefault(o => o["OrderSt"] != null && o["OrderSt"] == "101" && string.Compare(o.LeavesQty, "0") > 0);
//             if (ord != null)
//             {
//                 Console.WriteLine("User : {0}, 進行刪單作業, OrgSorRID={1}", client.User, ord.OrgSorRID);
//                 client.KillOrder(ord);
//                 Console.WriteLine("等待刪單回報後，再按 Enter 鍵繼續斷線作業");
//                 Console.ReadLine();
//             }
//             // 刪除委託簿剩餘委託單
//             client.KillAllOrder();
//             // 中斷交易主機連線
//             client.Disconnect();
//             client.OnApReady -= OnApReady;
//             client.OnOrderReport -= OnOrderReport;
//             Console.WriteLine("請按任意鍵結束程式");
//             Console.ReadKey();
//         }

//         /// <summary>
//         /// 連線完成事件通知
//         /// </summary>
//         static void OnApReady(object sender, EventArgs e)
//         {
//             Client client = sender as Client;
//             Console.WriteLine("User : {0}, 交易連線完成", client.User);

//             // 顯示可用帳號
//             Console.WriteLine("User : {0}, 可用交易帳號", client.User);
//             foreach (Acc acc in client.Accounts.Values)
//                 Console.WriteLine("User : {0}, BrkNo={1}, IvacNo={2}, SubacNo={3}", client.User, acc.BrkNo, acc.IvacNo, acc.SubacNo);

//             // 使用第一個可用帳號進行測試
//             var testAcc = client.Accounts.Count > 0 ? client.Accounts.Values[0] : null;
//             if (testAcc == null)
//                 return;

//             // 執行查詢作業
//             string taskID = "QINV",
//                 parmBrkNo = string.Format("bkno={0}", testAcc.BrkNo),
//                 parmIvac = string.Format("ivac={0}", testAcc.IvacNo);
//             Console.WriteLine("User : {0}, 發送查詢 taskID={1}", client.User, taskID);
//             // 執行 國內 - 未平倉餘額&明細查詢：作業代號 = QINV
//             client.SendSorTaskReq(taskID, new string[] { parmBrkNo, parmIvac, "QSUM=Y" });
//             //// 執行 國內 - 權益數查詢：作業代號 = QBal
//             //client.SendSorTaskReq("QBal", new string[] { parmBrkNo, parmIvac, "MCODE=NTX" });
//             //// 國外 - 未平倉餘額查詢：作業代號 = QINVF
//             //client.SendSorTaskReq("QINVF", new string[] { parmBrkNo, parmIvac });
//             //// 國外 - 權益數查詢：作業代號 = QBalF
//             //client.SendSorTaskReq("QBalF", new string[] { parmBrkNo, parmIvac, "CURRENCY=ALL" });
//             //// 國外 - 未平倉明細查詢：作業代號 = QOpnPosF
//             //client.SendSorTaskReq("QOpnPosF", new string[] { parmBrkNo, parmIvac });

//             // 進行新單測試
//             Console.WriteLine("User : {0}, 使用 BrkNo={1}, IvacNo={2}, SubacNo={3} 帳號進行新單測試", client.User, testAcc.BrkNo, testAcc.IvacNo, testAcc.SubacNo);
//             client.NewOrder(testAcc, "TwfNew", "B", "TXFG5", "R", "L", "9000", "1", "O");
//         }

//         /// <summary>
//         /// 回報事件通知
//         /// </summary>
//         static void OnOrderReport(Client client, SorOrd ord)
//         {   // 接收到委託回報資訊
//             Console.WriteLine("User : {0}, 委託回報 => OrgSorRID:{1}, OrgUser:{2}, LastMessage:{3}, OrderSt:{4}, ReqStep:{5}",
//                 client.User, ord.OrgSorRID, ord["OrgUser"], ord["LastMessage"], ord["OrderSt"], ord["ReqStep"]);
//         }
//     }

//     /// <summary>
//     /// 回報事件通知介面宣告
//     /// </summary>
//     delegate void OrderReportHander(Client client, SorOrd ord);

//     class Client
//     {
//         /// <summary>
//         /// 查詢作業流水序號
//         /// </summary>
//         int QuerySeqID_ = 0;

//         /// <summary>
//         /// 建構式
//         /// </summary>
//         /// <param name="caDllName">簽章元件檔案名稱</param>
//         public Client(string caDllName = null)
//         {
//             CaDllName_ = caDllName;

//             // 初始化 SorClient
//             InitializeSorClient();
//         }

//         /// <summary>
//         /// 連接登入 KLink 主機
//         /// </summary>
//         public void Connect(string server,string sysid, string user, string pass)
//         {
//             User = user;
//             SorClient_.Connect(server, sysid, user, pass);
//         }

//         /// <summary>
//         /// 中斷 KLink 主機連線
//         /// </summary>
//         public void Disconnect()
//         {
//             if (SorClient_.IsSessionConnected)
//                 SorClient_.Disconnect();
//             SorClient_.Dispose();
//         }

//         /// <summary>
//         /// 登入帳號
//         /// </summary>
//         public string User { get; private set; }

//         /// <summary>
//         /// 可用交易帳號
//         /// </summary>
//         public Accs Accounts { get; private set; }

//         /// <summary>
//         /// 委託清單
//         /// </summary>
//         public List<SorOrd> OrdsList
//         {
//             get
//             {
//                 return AllOrdsTable_.OrdsList;
//             }
//         }

//         public event EventHandler OnApReady = null;
//         public event EventHandler OnRecovered = null;
//         public event OrderReportHander OnOrderReport = null;

//         #region 建立 SorClient
//         /// <summary>
//         /// 簽章元件檔案名稱
//         /// </summary>
//         string CaDllName_;
//         /// <summary>
//         /// [委託表格/回報表格] 管理
//         /// </summary>
//         TablesMgr TablesMgr_ = new TablesMgr();

//         /// <summary>
//         /// 委託簿
//         /// </summary>
//         OrdsTable AllOrdsTable_ = new OrdsTable();

//         /// <summary>
//         /// 連線管理物件
//         /// </summary>
//         private SorClient SorClient_;

//         /// <summary>
//         /// 流量管制傳送輔助物件
//         /// </summary>
//         private SorFlowCtrlSender SorFlowSender_;

//         /// <summary>
//         /// 初始化 SorClient
//         /// </summary>
//         private void InitializeSorClient()
//         {
//             // 注意建構參數值決定其event callback時使用的執行序
//             SorClient_ = new SorClient(false);
//             SorClient_.OnSorConnectEvent = OnSorConnectEvent;
//             SorClient_.OnSorApReadyEvent = OnSorApReadyEvent;
//             SorClient_.OnSorChgPassResultEvent = OnSorChgPassResultEvent;
//             SorClient_.OnSorTaskResultEvent = OnSorTaskResultEvent;
//             SorClient_.OnSorRequestAckEvent = OnSorRequestAckEvent;
//             SorClient_.OnSorReportEvent = OnSorReportEvent;
//             SorFlowSender_ = new SorFlowCtrlSender(SorClient_);
//             Accounts = new Accs();
//         }

//         /// <summary>
//         /// Sor主機連接回覆
//         /// </summary>
//         void OnSorConnectEvent(SorClient sender, string errmsg)
//         {
//             if (!string.IsNullOrEmpty(errmsg))
//                 Console.WriteLine("User : {0}, 連接下單主機回覆訊息 : {1}", User, errmsg);
//         }

//         /// <summary>
//         /// 登入成功可以開始下單作業
//         /// </summary>
//         void OnSorApReadyEvent(SorClient sender)
//         {
//             #region Dump SgnResult
// #if DEBUG
//             using (System.IO.StreamWriter sw = new System.IO.StreamWriter("SgnResult.txt"))
//             {
//                 string outputData;
//                 foreach (string line in sender.SgnResult.OrigResult.Split('\x02'))
//                 {
//                     outputData = line;
//                     if (line.IndexOf('\x03') != -1)
//                         outputData = line.Replace("\x03", "").Replace("\x04", " ").Replace("\x0A", Environment.NewLine).Replace("\x01", Environment.NewLine);
//                     sw.WriteLine(outputData);
//                 }
//             }
// #endif
//             #endregion

//             SorTaskResult sgnResult = sender.SgnResult;
//             SorTable table;

//             #region 取出可用帳號.
//             table = sgnResult.NameTable("head");
//             if (table.IsInvalid)
//                 table = sgnResult.NameTable("mod");
//             int sgnact = 0;
//             int.TryParse(table.RecordField(0, table.Fields.NameField("sgnact")), out sgnact);

//             table = sgnResult.NameTable("Accs");
//             if (table.IsInvalid)
//                 table = sgnResult.NameTable("records");
//             Accounts.SorTableParser(table, CaDllName_, sgnact);
//             #endregion

//             #region  取出流量管制參數.
//             table = sgnResult.NameTable("FlowCtrl");
//             SorFields fields = table.Fields;
//             TIndex rate = 0;
//             TIndex rateMS = 0;
//             string fldRate = table.RecordField(0, fields.NameField("ORate"));
//             string fldRateMS = table.RecordField(0, fields.NameField("ORateMS"));
//             if (fldRate != null && fldRateMS != null)
//             {
//                 TIndex.TryParse(fldRate, out rate);
//                 TIndex.TryParse(fldRateMS, out rateMS);
//             }
//             SorFlowSender_.SetFlowCtrl(rate, rateMS);
//             if (rate <= 0 || rateMS <= 0)
//                 Console.WriteLine("User : {0}, 無流量管制參數", User);
//             else
//                 Console.WriteLine("User : {0}, 流量管制參數: {0}筆 / 每{1}{2}"
//                                                , User
//                                                , rate
//                                                , rateMS >= 1000 ? (rateMS / 1000.0) : rateMS
//                                                , rateMS >= 1000 ? "秒" : "ms");
//             #endregion

//             // 取出 [下單要求], [委託書], [委託回報] 欄位設定.
//             // 取出可用的 [改單要求]表 / [委託回報]表.
//             TablesMgr_.ParseSgnResult(sgnResult);

//             #region 發送回補作業
//             // 回補委託: 回補全部(「,D」 = 包含成交明細) (SendSorRequest() 必須保留前5碼).
//             // "-----1"                 回補全部，不包含成交明細
//             // "-----1" + "\x01" + "D"  補全部委託,含成交明細
//             // "-----1" + "\x01" + "Dw" 回補全部有剩餘量，並包含成交明細
//             // "-----1" + "\x01" + "M"  補有成交(或UserID相同)的委託,含成交明細
//             // "-----1" + "\x01" + "m"  補有成交(或UserID相同)的委託,不含成交明細
//             // "-----1" + "\x01" + "M0" 僅補有成交(不考慮UserID)的委託,含成交明細
//             // "-----1" + "\x01" + "m0" 僅補有成交(不考慮UserID)的委託,不含成交明細]
//             // "-----2" + "\x01" + "YYYYMMDDHHMMSS,D"  指定時間，包含成交明細
//             // "-----2" + "\x01" + "YYYYMMDDHHMMSS,Dw" 指定時間有剩餘量，包含成交明細
//             // "-----2" + "\x01" + "YYYYMMDDHHMMSS,m"  指定時間，僅回補有成交, 不包含成交明細
//             // "-----2" + "\x01" + "YYYYMMDDHHMMSS,M"  指定時間，僅回補有成交, 並包含成交明細
//             // "-----0"                 不回補,且不收任何回報
//             // "-----0m"                不回補、且不收委託回報，僅收成交回報
//             //sender.SendSorRequest(0x83, "-----1" + "\x01" + "D");

//             if (OnApReady != null)
//                 OnApReady(this, null);
//             #endregion
//         }

//         /// <summary>
//         /// 密碼變更作業回覆
//         /// </summary>
//         void OnSorChgPassResultEvent(SorClient sender, string user, string result)
//         {
//             Console.WriteLine("{0}: [{1}] 改密碼: {2}", DateTime.Now, user, string.IsNullOrEmpty(result) ? "成功" : ("失敗: " + result));
//         }

//         /// <summary>
//         /// 執行指定作業回覆
//         /// </summary>
//         /// <param name="sender"></param>
//         /// <param name="taskResult">SORS 作業結果分析</param>
//         void OnSorTaskResultEvent(SorClient sender, SorTaskResult taskResult)
//         {
//             // 請立即解析 & 處理, 處理太久會造成斷線喔!
//             System.Text.StringBuilder sb = new System.Text.StringBuilder();
//             TIndex T, tcount = taskResult.TablesCount;
//             sb.AppendLine(string.Format("OnSorTaskResult(作業結果分析): WorkID={0} , TablesCount:{1}", taskResult.WorkID, tcount));
//             for (T = 0; T < tcount; ++T)
//             {
//                 var table = taskResult.IndexTable(T);
//                 var tableName = table.Properties.Name;

//                 var fields = table.Fields;
//                 TIndex F, fcount = fields.Count, R, rcount = table.RecordsCount;
//                 if (tcount == 1 && fcount == 1 && rcount == 1)
//                 {
//                     var errMsgIdx = fields.NameFieldIndex("ErrMsg");
//                     if (errMsgIdx != SorField.InvalidIndex)
//                     {
//                         sb.AppendFormat("  作業失敗! ErrMsg={0}", table.RecordIndexField(0, 0));
//                         break;
//                     }
//                 }

//                 sb.AppendLine(string.Format("  TableName={0}, Fields={1}, Records={2}", tableName, fcount, rcount));
//                 if (fcount <= 0)
//                     continue;
//                 for (F = 0; F < fcount; ++F)
//                 {
//                     //var fields = fields.NameFieldIndex("BKNO");
//                     var field = fields.IndexField(F);
//                     //sb.AppendFormat("{0}{1}[{2}:{3}]", F == 0 ? "," : null, field.Properties.DisplayText, field.Properties.Name, field.Properties.Description);
//                     sb.AppendFormat("{0}{1}", F == 0 ? "   " : "," , field.Properties.Name);
//                 }
//                 sb.AppendLine();
//                 if (rcount <= 0)
//                     continue;
//                 for (R = 0; R < rcount; ++R)
//                 {
//                     if (R > 0)
//                         sb.AppendLine();
//                     for (F = 0; F < fcount; ++F)
//                     {
//                         sb.AppendFormat("{0}{1}", F == 0 ? "   " : ",", table.RecordIndexField(R, F));
//                     }
//                 }
//             }
//             Console.WriteLine("User : {0}, {1}", this.User, sb);
//         }

//         /// <summary>
//         /// Sor Request Ack
//         /// </summary>
//         /// <param name="sender"></param>
//         /// <param name="msgCode"></param>
//         /// <param name="acks"></param>
//         void OnSorRequestAckEvent(SorClient sender, TMsgCode msgCode, string acks)
//         {
//             Console.WriteLine("User : {0}, 主機回覆 MsgCode : 0x{1:x} , ACK訊息 : {2}", User, msgCode, acks);

//             if (msgCode == 0x83)
//             {
//                 if (OnRecovered != null && acks != null && !acks.StartsWith("ev"))
//                     OnRecovered(this, null);
//             }
//         }

//         /// <summary>
//         /// 委託回補 or 委託回報 or 成交回報
//         /// </summary>
//         void OnSorReportEvent(SorClient sender, string result)
//         {
//             string[] rs = result.Split('\n');
//             int rcount = rs.Length;
//             string orgSorRID;
//             SorOrd ord = null;
//             for (int L = 0; L < rcount; )
//             {
//                 string[] tnames = rs[L++].Split('\x01');
//                 string[] flds = rs[L++].Split('\x01');
//                 OrdTable ordTable = TablesMgr_.OrdTable(tnames[0]);
//                 if (ordTable == null)
//                 {
//                     if (tnames[0].Length == 0 && ord != null)
//                     {
//                         // 成交明細回補.
//                         ord.AddDealDetail(flds);
//                     }
//                     else//不認識的table清除ord,避免如果接下來有此筆[不認識的table的成交明細],會變成前一筆的明細!
//                         ord = null;
//                     continue;
//                 }
//                 if (tnames.Length <= 1)
//                 {
//                     // 委託回補.
//                     orgSorRID = ordTable.GetOrgSorRID(flds);
//                     if (orgSorRID != null)
//                         ord = AllOrdsTable_.AddSorOrd(ordTable, orgSorRID, flds, Accounts);
//                     continue;
//                 }
//                 // 委託回報.
//                 RptTable rptTable = TablesMgr_.RptTable(tnames[1]);
//                 if (rptTable != null)
//                 {
//                     orgSorRID = rptTable.GetOrgSorRID(flds);
//                     if (orgSorRID != null)
//                     {
//                         ord = AllOrdsTable_.SorOrdAtKey(orgSorRID);
//                         if (ord == null)//委託不存在,新增:
//                             ord = AllOrdsTable_.AddSorOrd(ordTable, orgSorRID, null, null);
//                         // 用回報資料填入委託書內容.
//                         ord.SetRptFields(rptTable, flds, Accounts);

//                         if (OnOrderReport != null)
//                             OnOrderReport(this, ord);
//                     }
//                 }
//             }
//         }
//         #endregion

//         #region 送單作業
//         /// <summary>
//         /// 下單流水號
//         /// </summary>
//         uint ReqSeqNo_ = 0;

//         /// <summary>
//         /// reqmsg1 為單筆訊息內容, 不包含 ReqSeqNo,
//         /// 例如: "\x01" + "3" + "\n" + "...各欄位內容..."
//         /// </summary>
//         void SendSorRequest(string reqmsg1, uint times)
//         {
//             TIndex start = ReqSeqNo_;
//             TIndex end;

//             if (times <= 0)
//                 return;
//             string reqbuf = "-----";
//             for (uint L = 0; L < times; ++L)
//                 reqbuf += string.Format(L == 0 ? "{0}{1}" : "\n{0}{1}", FetchReqSeqNo(), reqmsg1);
//             SendSorRequests(reqbuf);

//             end = ReqSeqNo_;
//             if ((end - start) == 1)
//                 Console.WriteLine("User : {0}, 送出Request {1}", User, end);
//             else if ((end - start) >= 1)
//                 Console.WriteLine("User : {0}, 送出Request {1} - {2}", User, ++start, end);
//         }

//         /// <summary>
//         /// 取得一個下單ACK序號.
//         /// </summary>
//         /// <returns></returns>
//         uint FetchReqSeqNo()
//         {
//             return ++ReqSeqNo_;
//         }

//         /// <summary>
//         /// 送出一筆 or 一批下單訊息,
//         /// reqbuf 需保留 5 bytes header, 每筆下單要求之間用 '\n' 分隔, 最後一筆不用加 '\n'
//         /// </summary>
//         void SendSorRequests(string reqbuf)
//         {
//             SorFlowSender_.SendSorRequests(reqbuf);
//         }
//         #endregion

//         #region 新單作業
//         public void NewOrder(Acc account, string tableName, string side, string symbol, string tif, string priType, string price, string qty, string possEff)
//         {
//             //REQ:TwfNew 台期權新單要求
//             SorApi.SorTaskResult sgnResult = SorClient_.SgnResult;
//             SorApi.SorTable table = sgnResult.NameTable("REQ:" + tableName);
//             SorApi.SorProperties prop = table.Properties;
//             string tableID = prop.Get("ID");
//             string[] flds = new string[table.Fields.Count];
//             flds[table.Fields.NameFieldIndex("BrkNo")] = account.BrkNo;
//             flds[table.Fields.NameFieldIndex("IvacNo")] = account.IvacNo;
//             flds[table.Fields.NameFieldIndex("SubacNo")] = account.SubacNo;
//             flds[table.Fields.NameFieldIndex("Side")] = side;
//             flds[table.Fields.NameFieldIndex("Symbol")] = symbol;
//             flds[table.Fields.NameFieldIndex("TIF")] = tif;
//             flds[table.Fields.NameFieldIndex("PriType")] = priType;
//             flds[table.Fields.NameFieldIndex("Price")] = price;
//             flds[table.Fields.NameFieldIndex("Qty")] = qty;
//             flds[table.Fields.NameFieldIndex("PosEff")] = possEff;

//             string errMsg;
//             var reqmsg = OrdTable.MakeRequestString(flds, tableID, account, table.Fields.NameFieldIndex("DigSgn"), out errMsg);
//             if (errMsg == null)
//                 SendSorRequest(reqmsg, 1);
//             else
//                 Console.WriteLine("User : {0}, NewOrder Failed, errMsg={1}", this.User, errMsg);
//         }
//         #endregion

//         #region 刪單作業
//         /// <summary>
//         /// 刪除剩餘口數大於0的委託單
//         /// </summary>
//         public void KillAllOrder()
//         {
//             KillOrder(AllOrderCondition);
//         }

//         /// <summary>
//         /// 判斷剩餘口數大於0判斷函式
//         /// </summary>
//         bool AllOrderCondition(SorOrd ord)
//         {
//             return string.Compare(ord.LeavesQty, "0") > 0;
//         }

//         /// <summary>
//         /// 針對符合條的的委託單進行批次刪單作業
//         /// </summary>
//         public void KillOrder(Func<SorOrd, bool> condition)
//         {
//             // 組刪單Request字串
//             string reqbuf = "-----";
//             int count = 0;
//             foreach (SorOrd ord in AllOrdsTable_.OrdsList)
//             {
//                 ReqKillTable reqkt = ord.Table.ReqKillTable;
//                 if (reqkt != null && condition(ord))
//                 {
//                     string errMsg;
//                     var reqmsg = reqkt.MakeKillReqStr(ord, out errMsg);
//                     if (errMsg != null)
//                     {
//                         Console.WriteLine("User : {0}, KillOrder Failed, errMsg={1}", this.User, errMsg);
//                         continue;
//                     }

//                     reqbuf += string.Format(count == 0 ? "{0}{1}" : "\n{0}{1}", FetchReqSeqNo(), reqmsg);
//                     ++count;
//                 }
//             }
//             // 送出刪單訊息字串.
//             if (reqbuf.Length > 5)
//             {
//                 Console.WriteLine("User : {0}, 送出Request {1}", User, ReqSeqNo_);
//                 SendSorRequests(reqbuf);
//             }
//         }

//         /// <summary>
//         /// 針對特定委託進行刪單作業
//         /// </summary>
//         /// <param name="ord"></param>
//         public void KillOrder(SorOrd ord)
//         {
//             // 組刪單Request字串
//             string reqbuf = "-----";
//             ReqKillTable reqkt = ord.Table.ReqKillTable;
//             string errMsg;
//             var reqmsg = reqkt.MakeKillReqStr(ord, out errMsg);
//             if (errMsg != null)
//             {
//                 Console.WriteLine("User : {0}, KillOrder Failed, errMsg={1}", this.User, errMsg);
//                 return;
//             }
//             reqbuf += string.Format("{0}{1}", FetchReqSeqNo(), reqmsg);
//             // 送出刪單訊息字串.
//             Console.WriteLine("User : {0}, 送出Request {1}", User, ReqSeqNo_);
//             SendSorRequests(reqbuf);
//         }
//         #endregion

//         #region 執行指定作業(查詢)
//         /// <summary>
//         /// 執行指定作業(查詢)
//         /// </summary>
//         /// <param name="taskID">作業名稱</param>
//         /// <param name="parms">作業參數</param>
//         public void SendSorTaskReq(string taskID, string[] parms)
//         {
//             var SOH = "\x01";
//             var workid = string.Format("qid{0}", ++QuerySeqID_);
//             string reqstr = string.Format("{1}{0}{2}{0}{3}", SOH, workid, taskID, string.Join(SOH, parms));
//             if (SorClient_.SendSorRequest(0x80, "-----" + reqstr))
//                 Console.WriteLine("User : {0}, 送出 0x80 Request {1}", User, workid);
//             else
//                 Console.WriteLine("User : {0}, 查詢傳送失敗(可能原因:流量管制或斷線) {1}", User, reqstr);
//         }
//         #endregion
//     }
// }

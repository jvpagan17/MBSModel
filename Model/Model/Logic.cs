
/*unit Logic;

interface

Uses
 SysUtils, Math;

Const
 MAXTRANCHES = 100;
MAXPACTRANCHES = 30;
 MAXPERIODS = 360;
 SCENARIOS = 7;
 MAXLIKETRANCHES = 10;
 MAXCOLLATERALPOOLS = 10000;
 type
  CollCashFlowsType = Record
      bal, sprin, uprin, inter, serv: double;
      end;
  ArrayCollCashFlowsType = array[0..MAXPERIODS] of CollCashFlowsType;

  TrancheCashFlowsType = Record
      bal, prin, inter, cash: double;
      end;
  ArrayTrancheCashFlowsType = array[0..MAXTRANCHES, 0..MAXPERIODS] of TrancheCashFlowsType;
  LikeRecordType = Record
      tranche:integer;
      amt:double;
      end;
  LikeArrayType = array[0..MAXLIKETRANCHES] of LikeRecordType;
  TrancheDataType = Record
      dflt, fmat, lmat, PAC, fserial, lserial: Integer;
      amt, origamt, coupon, yld, spread, price, pricepct,
       avglife, tkdwn, dtkdwn, cyld, dur, mdur, zaccrued,
       cwac, swac, kwac: double;
      serial, zbond, py, Sub, Mez, io, wac: boolean;
      name,ttype: String[3];
      like: LikeArrayType;
      end;
  PACDatatype = Record
    TrancheNumber:integer;
    amt:double;
    end;
  PACTableType = Record
    prin, bal:double;
    end;
  ArrayTrancheDataType = array[0..MAXTRANCHES] of TrancheDataType;
  ArrayPACDataType = array[0..MAXTRANCHES] of PACDataType;

  ReserveArrayType = Array[0..MAXPERIODS] of double;
  BufferArrayType = Array[0..MAXTRANCHES] of double;
  SpeedArrayType = Array[1..SCENARIOS] of double;
  ModelArrayTYPE = Array[1..SCENARIOS] of integer;
  PACTableArrayType = Array[0..30, 0..MAXPERIODS] of PACTableType;
  PACUseTableType = Array[0..30, 0..MAXPERIODS] of double;
  SeniorSubShiftTableType = Array[0..1, 0..MAXPERIODS] of double;
var

  cf : ArrayCollCashFlowsType;
  tf : ArrayTrancheCashFlowsType;
  td : ArrayTrancheDataType;
  pd : ArrayPACDataType;
  daysaccrued, days1stprd, CollateralCost, CollateralAccrued,
   SeniorBal, SeniorWAC, SubBal, MezBal, SeniorPct, SubPct, MezPct,
   InitialSeniorPct, zaccrued, pdprin, pdint, InitialTotBonds,
   PrecisionFactor, CollateralWAC, CollateralWAM, CollateralPT: double;
  NumberOfTranches, NumberOfPACTranches, CurrentTranche : Integer;
  SeniorFlag: Boolean;
  SeniorPrin, JuniorPrin, MezExtraPrin, residual, reserve, expenses,
  ReinvIncome: ReserveArrayType;
  SpeedArray: SpeedArrayType; {For Price/Yield Matrix}
  ModelArray: ModelArrayType; {For Price/Yield Matrix}
  PACTableArray: PACTableArrayType;
  PACUseTable: PACUseTableType;
  SeniorSubShiftTable: SeniorSubShiftTableType;

Procedure CalcCollateralCashFlows(Speed:double; ModelIndex:Integer);
Procedure CalcTrancheCashFlows;
Procedure CalcTrancheStatistics;
Procedure SaveTrancheData;
Procedure CalcNetProfitability;
Procedure LoadCollateralCashFlows;
Procedure LoadTrancheCashFlows;
Procedure LoadExpensesCashFlows;
Procedure LoadIncomeCashFlows;
Procedure CalcDates;
Procedure CalcPACTableCashFlows;
Procedure LoadPACCashFlowsGrid(s:string);
Procedure LoadPACUseTable;
Procedure CalcSeniorSubShiftTable;
Procedure CalcSeniorSubPrincipal(n:integer);
Procedure Prinpay(var Source:double; i,n:integer;UntilAmt:double;A:LikeArrayType);
Procedure CalcLikeArray(i:integer; pct:double;i1:integer);
Procedure GotoLocation;
Procedure GetLocation;
Procedure AdjustTrancheCashFlows(i:integer; fserialskip,lserialskip:double);
Procedure LoadPriceYieldCashFlows(i, s:integer);
Procedure CalcStructureForm;

function calcprice(i1:integer; yield1,amount,precision:double):double;
function calcyield(i1:integer; proceeds1:double):double;
function calcduration(i1:integer; yield1:double):double;
function LoadTrancheData:integer;
function MonthToDate(m:integer):String;
function PriceTo32nds(const price1:double):string;
function calcavglife(i1:integer):double;
function calcfmat(i1:integer):integer;
function calclmat(i1:integer):integer;
function calcdflt(i1:integer):integer;
function CalcTsyRate(avglife:double; mode:integer):double;
function my_power(x:double; y:double):double;
function LoadPACData:integer;
function Days360(date1, date2:string):integer;
function ReadPrecision(pricestring:string; precision:double):double;
function WritePrecision(price, precision:double):string;
function Max(A:Double; B:Double):Double;
function Min(A:Double; B:Double):Double;
function CalcCollCertExcess:Double;

implementation

uses
Structure, Collateral, ViewCollCashFlows, ViewTrancheCashFlows,
ViewExpensesCashFlows, ViewIncomeCashFlows, Economy, Update,
PACFinder, ViewPACCashFlows, PACTable, SeniorSub, PriceYieldMatrix,
ViewPriceYieldCashFlows;

Procedure CalcCollateralCashFlows(Speed:double; ModelIndex:Integer);

Var
n, FloatDays, PoolType, OTerm, LastPeriod, Balloon, RTerm, WALA, UpdateAge, PoolModel :integer;
FloatRate, SchedPrincipal, UnschedPrincipal, Interest, Servicing, Balance, CalcBal, Cpr,
 Buffer, SerialBalance, SerialPrincipal, GCpn, PT, Price,
 FSerial, LSerial, TSerial, FUnit, LUnit, PoolSpeed, MRR: double;
year1, month1, day1, year2, month2, day2: word;
PoolDesc, GNMAType: String;
begin {Procedure TStructureForm.CalcClick}
  {Collateral Cash Flow Calculations }
  fillchar(cf,sizeof(cf),0);
  fillchar(ReinvIncome,sizeof(ReinvIncome),0);
  CollateralWAC:=0;
  CollateralWAM:=0;
  CollateralPT:=0;
  CollateralCost:=0;
  CollateralAccrued:=0;
  CalcDates();
  DecodeDate(StrToDate(StructureForm.FirstPaymentDate.Text),year1,month1,day1);
  DecodeDate(StrToDate(StructureForm.FirstPaymentDateUpdate.Text),year2,month2,day2);
  UpdateAge:=year2*12+month2-(year1*12+month1);
  with CollateralForm.CollateralTable do
  begin
    First;
DisableControls;
    {Loop: Reads each collateral record}
    while Not(EOF) do
    begin
      Buffer:=0;
      {Read Fields from Database}
      if StructureForm.UpdateComboBox.ItemIndex=1 then
      begin
        Balance:=FieldByName('UBalance').AsFloat;
        CalcBal:=FieldByName('UCalcBal').AsFloat;
        RTerm:=FieldByName('RTerm').AsInteger-UpdateAge;
        WALA:=FieldByName('WALA').AsInteger+UpdateAge;
      end
      else
      begin
        Balance:=FieldByName('Balance').AsFloat;
        CalcBal:=FieldByName('CalcBal').AsFloat;
        RTerm:=FieldByName('RTerm').AsInteger;
        WALA:=FieldByName('WALA').AsInteger;
      end;
      PoolType:=FieldByName('Type').AsInteger;
      OTerm:=FieldByName('OTerm').AsInteger;
      GCpn:=FieldByName('GCpn').AsFloat;
      PT:=FieldByName('P/T').AsFloat;
      FSerial:=FieldByName('1st Serial').AsInteger;
      LSerial:=FieldByName('Lst Serial').AsInteger;
      TSerial:=FieldByName('Tot Serial').AsInteger;
      Balloon:=FieldByName('Balloon').AsInteger;
      Price:=FieldByName('Price').AsFloat;
      SerialBalance:=CalcBal;
      {Determine Last Payment Date, check for Ballon Payments}
      If OTerm = 0 Then OTerm:=RTerm + WALA;
      If Balloon>0 Then LastPeriod:=Balloon-(OTerm-RTerm)
      Else LastPeriod:=RTerm;

      //Calculate Float Days for investment earnings at 2.5% or 3.0% on TracieMaes
      PoolDesc:=FieldByName('Desc').AsString;
      GNMAType:=PoolDesc[1]+PoolDesc[2];
      If pos('\TM', CollateralFile)>0 then FloatRate:=0 else FloatRate:=0;
      FloatDays:=0;
      if GNMAType='GN' then FloatDays := 16;
      if GNMAType='G2' then FloatDays := 11;

      // Adjust PoolModel and PoolSpeed if prepayment model is MDL
      MRR:=9.125;  // Define Market Refinance Rate for MDL
      if ModelIndex=2 then
      begin
        PoolModel:=0;
        if WALA<31 then
        begin
          if GCpn<(MRR-1) then PoolSpeed:=120/100.0* Speed else
          if GCpn<(MRR-0.5) then PoolSpeed:=146/100.0* Speed else
          if GCpn<(MRR-0) then PoolSpeed:=169/100.0* Speed else
          if GCpn<(MRR+0.5) then PoolSpeed:=226/100.0* Speed else
          if GCpn<(MRR+1) then PoolSpeed:=288/100.0* Speed else
          if GCpn<(MRR+2) then PoolSpeed:=323/100.0* Speed else
          if GCpn<(MRR+3) then PoolSpeed:=399/100.0* Speed else
          if GCpn<(MRR+4) then PoolSpeed:=496/100.0* Speed else
          PoolSpeed:=536/100.0* Speed;
end
        else
        if WALA<61 then
        begin
          if GCpn<(MRR-1) then PoolSpeed:=133/100.0* Speed else
          if GCpn<(MRR-0.5) then PoolSpeed:=155/100.0* Speed else
          if GCpn<(MRR-0) then PoolSpeed:=170/100.0* Speed else
          if GCpn<(MRR+0.5) then PoolSpeed:=194/100.0* Speed else
          if GCpn<(MRR+1) then PoolSpeed:=236/100.0* Speed else
          if GCpn<(MRR+2) then PoolSpeed:=299/100.0* Speed else
          if GCpn<(MRR+3) then PoolSpeed:=399/100.0* Speed else
          if GCpn<(MRR+4) then PoolSpeed:=475/100.0* Speed else
          PoolSpeed:=496/100.0* Speed;
end
        else
        begin
          if GCpn<(MRR-1) then PoolSpeed:=139/100.0* Speed else
          if GCpn<(MRR-0.5) then PoolSpeed:=159/100.0* Speed else
          if GCpn<(MRR-0) then PoolSpeed:=173/100.0* Speed else
          if GCpn<(MRR+0.5) then PoolSpeed:=189/100.0* Speed else
          if GCpn<(MRR+1) then PoolSpeed:=222/100.0* Speed else
          if GCpn<(MRR+2) then PoolSpeed:=257/100.0* Speed else
          if GCpn<(MRR+2) then PoolSpeed:=321/100.0* Speed else
          if GCpn<(MRR+4) then PoolSpeed:=324/100.0* Speed else
          PoolSpeed:=340/100.0* Speed;
end
end
      else
      begin
        PoolModel:=ModelIndex;
        PoolSpeed:=Speed;
      end;

      {Loop: Calculates cash flows for the collateral record}
      For n:=1 to LastPeriod do
      begin

        {Calculates scheduled and unschenduled principal payments}
        If n = LastPeriod Then
          begin
          SchedPrincipal:=Balance;
          UnschedPrincipal:=0;
        end
        Else
        begin   //Prepayment Model
          SchedPrincipal:=GCpn/1200* Balance/(power(1+GCpn/1200, RTerm-n+1)-1);
          If PoolModel = 1 Then Cpr:=1 Else
            If(WALA+n)>30 Then Cpr:=0.06 Else Cpr:=0.002*(WALA+n);
          UnSchedPrincipal:=(Balance-SchedPrincipal)*(1.0-power(1.0-Cpr* PoolSpeed/100,1.0/12));
        end;

        {Calculation of serial principal payments}
        if n=RTerm Then SerialPrincipal:=SerialBalance
        Else
        begin
          FUnit:=TSerial+1-int((balance+buffer
          +0.001)/25000);
          LUnit:=FUnit+int((SchedPrincipal+UnschedPrincipal
           +buffer+0.001)/25000)-1;
          If LUnit = TSerial Then LUnit:=LUnit-1;
          SerialPrincipal:=25000* Max(0, Min(LUnit, LSerial)
           -Max(FUnit, FSerial)+1);
        end;
        Buffer:=buffer+SchedPrincipal+UnschedPrincipal
         -int((buffer+SchedPrincipal+UnschedPrincipal)/25000)*25000;
        If PoolType = 1 Then
         begin
          Interest:=PT/1200* SerialBalance;
Servicing:=(Gcpn-PT)/1200* SerialBalance;
end Else
        begin
          Interest:=PT/1200* balance;
Servicing:=(Gcpn-PT)/1200* balance;
end;
         SerialBalance:=SerialBalance-SerialPrincipal;
        Balance:=Balance-SchedPrincipal-UnschedPrincipal;
        If PoolType = 1 Then
         begin
          SchedPrincipal:=SerialPrincipal;
           UnschedPrincipal:=0;
        end;

        if PoolType=1 Then cf[n].bal:=cf[n].bal+SerialBalance
        Else cf[n].bal:=cf[n].bal+Balance;
        cf[n].sprin:=cf[n].sprin+SchedPrincipal;
        cf[n].uprin:=cf[n].uprin+UnschedPrincipal;
        cf[n].inter:=cf[n].inter+Interest;
        cf[n].serv:=cf[n].serv+Servicing;

        // Add float investment income for TracieMaes at 3.0% or 2.5% based on collateral description of GN or G2
        ReinvIncome[n]:=ReinvIncome[n]+(SchedPrincipal+UnschedPrincipal+Interest)* FloatDays/360* FloatRate/100;

      end; {Loop: Calculation of cash flows for each collateral record}

      {Calculations done once for each collateral record}
      cf[0].bal:=cf[0].bal+CalcBal;
      CollateralCost:=CollateralCost+CalcBal* Price/100;
      CollateralAccrued:=CollateralAccrued+PT/1200* CalcBal* daysaccrued/30;
      CollateralWAC:=CollateralWAC+CalcBal* GCpn;
CollateralWAM:=CollateralWAM+CalcBal* RTerm;
CollateralPT:=CollateralPT+CalcBal* PT;
Next;
    end;  {Loop: Reads each collateral record}
    EnableControls;
    First;
    CollateralWAC:=CollateralWAC/cf[0].bal;
    CollateralWAM:=CollateralWAM/cf[0].bal;
    CollateralPT:=CollateralPT/cf[0].bal;
  end;    {With CollateralForm do}

end;

Procedure CalcSeniorSubShiftTable();
Var
i, n:integer;
begin

  // Calculate Initial Senior Percent
  SeniorBal:=0;
  MezBal:=0;
  SubBal:=0;
    for i:=1 to NumberOfTranches do if td[i].io=false then
   if td[i].Sub then SubBal:=SubBal+td[i].origamt
   else if td[i].Mez then MezBal:=MezBal+td[i].origamt
   else SeniorBal:=SeniorBal+td[i].origamt;
  if (SeniorBal+SubBal+MezBal)>0 then
  InitialSeniorPct:=SeniorBal/(SeniorBal+SubBal+MezBal);

  with SeniorSubForm do
  begin
    n:=1;
    while n <= MAXPERIODS do
    begin
      if n <= StrToInt(PrdEdit1.Text) then
      begin
        SeniorSubShiftTable[0, n]:=StrToInt(SeniorEdit1.Text);
SeniorSubShiftTable[1, n]:=StrToInt(MezEdit1.Text);
end else if n <= (StrToInt(PrdEdit1.Text)+StrToInt(PrdEdit2.Text)) then
begin
        SeniorSubShiftTable[0, n]:=StrToInt(SeniorEdit2.Text);
SeniorSubShiftTable[1, n]:=StrToInt(MezEdit2.Text);
end else if n <= (StrToInt(PrdEdit1.Text)+StrToInt(PrdEdit2.Text)+
      StrToInt(PrdEdit3.Text)) then
      begin
        SeniorSubShiftTable[0, n]:=StrToInt(SeniorEdit3.Text);
SeniorSubShiftTable[1, n]:=StrToInt(MezEdit3.Text);
end else if n <= (StrToInt(PrdEdit1.Text)+StrToInt(PrdEdit2.Text)+
      StrToInt(PrdEdit3.Text)+StrToInt(PrdEdit4.Text)) then
      begin
        SeniorSubShiftTable[0, n]:=StrToInt(SeniorEdit4.Text);
SeniorSubShiftTable[1, n]:=StrToInt(MezEdit4.Text);
end else if n <= (StrToInt(PrdEdit1.Text)+StrToInt(PrdEdit2.Text)+
      StrToInt(PrdEdit3.Text)+StrToInt(PrdEdit4.Text)+StrToInt(PrdEdit5.Text)) then
      begin
        SeniorSubShiftTable[0, n]:=StrToInt(SeniorEdit5.Text);
SeniorSubShiftTable[1, n]:=StrToInt(MezEdit5.Text);
end else if n <= (StrToInt(PrdEdit1.Text)+StrToInt(PrdEdit2.Text)+
      StrToInt(PrdEdit3.Text)+StrToInt(PrdEdit4.Text)+StrToInt(PrdEdit5.Text)
      +StrToInt(PrdEdit6.Text)) then
      begin
        SeniorSubShiftTable[0, n]:=StrToInt(SeniorEdit6.Text);
SeniorSubShiftTable[1, n]:=StrToInt(MezEdit6.Text);
end else
      begin
        SeniorSubShiftTable[0, n]:=StrToInt(SeniorEdit7.Text);
SeniorSubShiftTable[1, n]:=StrToInt(MezEdit7.Text);
end;
      n:=n+1;
    end;
  end;
end;

Procedure CalcSeniorSubPrincipal(n:integer);
Var
i:integer;
begin
  SeniorBal:=0;
  MezBal:=0;
  SubBal:=0;
  for i:=1 to NumberOfTranches do if td[i].io=false then
   if td[i].Sub then SubBal:=SubBal+tf[i, n - 1].bal
   else if td[i].Mez then MezBal:=MezBal+tf[i, n - 1].bal
   else
  begin
     SeniorBal:=SeniorBal+tf[i, n - 1].bal;
     SeniorWAC:=SeniorWAC+tf[i, n - 1].bal* td[i].coupon;
  end;
  If SeniorBal>0.01 then SeniorWAC:=SeniorWAC/SeniorBal
   else SeniorWAC:=0;
  if (SeniorBal+SubBal+MezBal)>0 then
  begin
    SeniorPct:=SeniorBal/(SeniorBal+SubBal+MezBal);
    MezPct:=MezBal/(SeniorBal+SubBal+MezBal);
    SubPct:=SubBal/(SeniorBal+SubBal+MezBal);
  end else
  begin
    SeniorPct:=0;
    MezPct:=0;
    SubPct:=0;
  end;
  if SeniorSubForm.CheckBox1.Checked and(SeniorPct>InitialSeniorPct) then
  SeniorSubShiftTable[0, n]:=100;
  SeniorPrin[n]:=(cf[n].sprin+zaccrued)* SeniorPct
   +cf[n].uprin*(SeniorPct+(1-SeniorPct)* SeniorSubShiftTable[0, n]/100);
  MezExtraPrin[n]:=cf[n].uprin* SeniorSubShiftTable[1, n]/100*
   (1-(SeniorPct+(1-SeniorPct)* SeniorSubShiftTable[0, n]/100));
  JuniorPrin[n]:=cf[n].sprin+zaccrued+cf[n].uprin-SeniorPrin[n]-MezExtraPrin[n];
end;


Procedure CalcTrancheCashFlows();    {Tranche Cash Flow Calculations}

Var
c, n, i, j, t:Integer;
pct, classprin, classint, serialamt, tbondbegprd, reremicexpenses, reremicbalance,
 reservechange, expenseamt, expenserate, seniorexpenserate: double;
tbuffer: BufferArrayType;

begin
  seniorbal:=0; // Used for seniorexpenserate*seniorbal if not SeniorFlag
  calcdates();
with StructureForm do
  begin
    expenseamt:=StrtoFloat(ExpenseAmtTEdit.Text);
expenserate:=StrtoFloat(ExpenseRateTEdit.Text)/10000;
    seniorexpenserate:=StrtoFloat(SeniorSubForm.SeniorExpenseRateTEdit.Text)/10000;
    if StructureForm.UpdateComboBox.ItemIndex<1 then
    reserve[0]:=StrtoFloat(InitialReserve.Text)
    else reserve[0]:=StrtoFloat(InitialReserveUpdate.Text)
  end;

  NumberOfTranches:=LoadTrancheData;
  fillchar(tf,sizeof(tf),0);
  fillchar(tbuffer,sizeof(tbuffer),0);
  fillchar(SeniorPrin,sizeof(SeniorPrin),0);
  fillchar(MezExtraPrin,sizeof(MezExtraPrin),0);
  fillchar(JuniorPrin,sizeof(JuniorPrin),0);
  c:=0;
  for i:=1 to NumberOfTranches do
  begin
    tf[i, 0].bal:= td[i].amt;
    if not(td[i].io) then tf[0, 0].bal:= tf[0, 0].bal+td[i].amt;
  end;
  for i:=1 to NumberOfTranches do // Adds tranche serial buffer to first paying tranche in update mode.
  begin
    if ((StructureForm.UpdateComboBox.ItemIndex=1)and(c= 0)and td[i].serial
      and(td[i].amt>0))then tbuffer[i]:=Max(0, Min(tf[0, 0].bal-cf[0].bal+CalcCollCertExcess(),25000));
    if td[i].amt>0 then c:=c+1;
  end;
  If SeniorFlag then CalcSeniorSubShiftTable;
  for n:=1 to MAXPERIODS do
  begin
    reserve[n]:=reserve[n - 1];
    tbondbegprd:=0;
    zaccrued:=0;
    for i:=1 to NumberOfTranches do
    begin
      if td[i].zbond=True then
      begin
        td[i].zaccrued:=tf[i][n - 1].bal* td[i].coupon/1200;
        zaccrued:=zaccrued + td[i].zaccrued;
        tf[i][n].prin:= - td[i].zaccrued;
      end;
    end;
    // Pay VADM here - to be added
    pdint:=cf[n].inter;
    if SeniorFlag=true then
    begin
     CalcSeniorSubPrincipal(n);  // zaccrued treated as sprin
pdprin:=SeniorPrin[n];
    end else
    pdprin:=cf[n].sprin+cf[n].uprin + zaccrued;
    // Pay Interest First
    for i:=1 to NumberOfTranches do
    begin
      if td[i].WAC and(cf[n - 1].bal>0) then
        classint:=((cf[n].inter/cf[n - 1].bal*1200)* td[i].cwac/100
       +seniorwac* td[i].swac/100+td[i].kwac)* tf[i][n-1].bal/1200
      else classint:=tf[i][n - 1].bal* td[i].coupon/1200;
      if i<NumberOfTranches then   // Used for excess servicing valuations
        tf[i][n].inter:=Max(Min(classint, pdint),0)
        else tf[i][n].inter:=Min(classint, pdint);  // Allows for negative excess servicing fee
pdint:=pdint-tf[i][n].inter;
      reservechange:=Min(reserve[n], classint-tf[i][n].inter);
tf[i][n].inter:=tf[i][n].inter+reservechange;
      reserve[n]:=reserve[n]-reservechange;
    end;
    // Pay Principal
    if StructureForm.AmortizeComboBox.ItemIndex=1 then // Change Amortz to Cash
    begin
      pdprin:=pdprin+pdint;
      pdint:=0;
    end;
    for i:=1 to NumberOfTranches do if td[i].name='B~' then // R&G 92 GT1 Modification to class B - Pay $5k + coupon on amortized portion of B
    begin
      tf[i][n].prin:=min(pdprin, min((5000+td[i].coupon*(2340117.55-tf[i][n - 1].bal)/1200),tf[i][n - 1].bal));
      pdprin:=pdprin-tf[i][n].prin;
    end;
    for i:=1 to NumberOfTranches do if ((td[i].ttype='IND') and
     not(td[i].Sub or td[i].Mez)) then
    Prinpay(pdprin, i, n,0, td[i].like);
    for i:=1 to NumberOfTranches do if ((td[i].PAC>0) and(td[i].ttype= 'S/F')
     and not(td[i].Sub or td[i].Mez)) then
    Prinpay(pdprin, i, n, PACUseTable[td[i].PAC, n], td[i].like);
    for i:=1 to NumberOfTranches do if (Not((td[i].ttype= 'S/F')or
     (td[i].ttype= 'SM')or(td[i].serial))and not(td[i].Sub or td[i].Mez)) then
      Prinpay(pdprin, i, n,0, td[i].like)
     else if (td[i].serial and not(td[i].Sub or td[i].Mez)) then
   begin
      classprin:=Min(tf[i][n - 1].bal-tf[i][n].prin-tbuffer[i], pdprin);
serialamt:=min(-tf[i][n].prin, classprin+tbuffer[i]) //pay zaccrual first
       +int(max(classprin+tf[i][n].prin+tbuffer[i]+0.001,0)/25000)*25000;
      if pdprin>=(tf[i][n - 1].bal-tf[i][n].prin-tbuffer[i])then
        begin
       tf[i][n].prin:=tf[i][n].prin+classprin+tbuffer[i];
       tbuffer[i]:=0;
      end else if not(td[i].Sub or td[i].Mez) then
     begin
        tf[i][n].prin:=tf[i][n].prin+serialamt;
        tbuffer[i]:=tbuffer[i]+classprin-serialamt;  //serialamt is actual cash being paid
      end;
      pdprin:=pdprin-classprin;
    end;
    for i:=1 to NumberOfTranches do if ((td[i].PAC>0) and(td[i].ttype= 'S/F')
     and not(td[i].Sub or td[i].Mez)) then
    Prinpay(pdprin, i, n,0, td[i].like);

    if SeniorFlag then
      begin
      pdprin:=pdprin+MezExtraPrin[n];
      if (MezPct+SubPct)>0 then
       pdprin:=pdprin+JuniorPrin[n]*(MezPct/(MezPct+SubPct));
      for i:=1 to NumberOfTranches do if (td[i].Mez and not(td[i].ttype= 'S/F'))then
        Prinpay(pdprin, i, n,0, td[i].like);
      if (MezPct+SubPct)>0 then
       pdprin:=pdprin+JuniorPrin[n]*(SubPct/(MezPct+SubPct));
      for i:=1 to NumberOfTranches do if (td[i].Sub and not(td[i].ttype= 'S/F'))then
        Prinpay(pdprin, i, n,0, td[i].like);
end;

    for i:=1 to NumberOfTranches do
    begin
      tf[i][n].bal:=tf[i][n - 1].bal-tf[i][n].prin;
      tf[i][n].cash:=tf[i][n].prin+tf[i][n].inter;

      tf[0][n].prin:=tf[0][n].prin+tf[i][n].prin;
      tf[0][n].inter:=tf[0][n].inter+tf[i][n].inter;
      tf[0][n].bal:=tf[0][n].bal+tf[i][n].bal;
      tf[0][n].cash:=tf[0][n].cash+tf[i][n].cash;

      tbondbegprd:=tbondbegprd+tf[i][n - 1].bal;
    end;


    // Total Bonds Interest does not include residual cashflow
    tf[0][0].prin:=tf[0][0].prin+tf[0][n].prin;
    tf[0][0].inter:=tf[0][0].inter+tf[0][n].inter;
    tf[0][0].cash:=tf[0][0].cash+tf[0][n].cash;

    SeniorPrin[0]:=SeniorPrin[0]+SeniorPrin[n];
    MezExtraPrin[0]:=MezExtraPrin[0]+MezExtraPrin[n];
    JuniorPrin[0]:=JuniorPrin[0]+JuniorPrin[n];

    // Re-REMIC Expenses used for R&G 95_1 GNMA Trust
    reremicbalance:=0;
    reremicexpenses:=0;
    for i:=1 to NumberOfTranches do if td[i].name='S~' then
      reremicbalance:=reremicbalance+tf[i, n - 1].bal;
    if reremicbalance>0 then reremicexpenses:=reremicbalance*0.0002+2000;

    // Expenses adjusted for TracieMaes - use collateral balance at end of period instead of bonds outstanding at beginning of period
    // All files that start with \TM are assumed to be TracieMaes
    If pos('\TM', CollateralFile)>0 then
       expenses[n]:=Min((expenseamt+expenserate* cf[n-1].bal)/12,pdint)
      else
      expenses[n]:=Min((expenseamt+expenserate* tbondbegprd+seniorexpenserate* seniorbal+reremicexpenses)/12,pdint);
    pdint:=pdint-expenses[n]+ReinvIncome[n];
    residual[n]:=pdint+pdprin;
    tf[NumberOfTranches][n].inter:=tf[NumberOfTranches][n].inter+residual[n];
    tf[NumberOfTranches][n].cash:=tf[NumberOfTranches][n].cash+residual[n];

    for i:=1 to NumberOfTranches do
    begin
      tf[i, 0].prin:=tf[i, 0].prin+tf[i, n].prin;
      tf[i, 0].inter:=tf[i, 0].inter+tf[i, n].inter;
      tf[i, 0].cash:=tf[i, 0].cash+tf[i, n].cash;
    end;
    for i:=1 to NumberOfTranches do if td[i].io then
    begin
      if td[i].ttype='IND' then
       tf[i, n].bal:=tf[i, n - 1].bal-td[i].origamt/InitialTotBonds*
       (cf[n].sprin+cf[n].uprin + zaccrued);
      if td[i].ttype='SM' then
      begin
        tf[i, n].bal:=tf[i, n - 1].bal;
        for j:=1 to td[i].like[0].tranche do
        begin
          t:=td[i].like[j].tranche;
          pct:=td[i].like[j].amt;
          tf[i, n].bal:=tf[i, n].bal-pct* td[i].origamt/td[t].origamt* tf[t, n].prin;
        end;
      end;
    end;
  end;
end;

Procedure Prinpay(var Source:double; i,n:integer;UntilAmt:double;A:LikeArrayType);
var
j:integer;
classprin:double;
begin
  if td[i].origamt>0 then
  begin
    if td[i].ttype='IND' then
     classprin:=Min(Source*(td[i].origamt/InitialTotBonds),
     tf[i, n - 1].bal-tf[i, n].prin-UntilAmt)
     else
     classprin:=Min(Source*(td[i].origamt/(td[i].origamt+A[0].amt)),
     tf[i, n - 1].bal-tf[i, n].prin-UntilAmt);
    tf[i][n].prin:=tf[i][n].prin + classprin;
    if A[0].tranche>0 then for j:=1 to A[0].tranche do
    begin
      tf[A[j].tranche][n].prin:=tf[A[j].tranche][n].prin+
       classprin/td[i].origamt* A[j].amt;
    end;
    Source:=Source-classprin*((td[i].origamt+A[0].amt)/td[i].origamt);
  end;
end;



Procedure CalcTrancheStatistics();

Var
i1:integer;

begin
  For i1:=1 to NumberOfTranches do
  begin
    if td[i1].wac then td[i1].coupon:=tf[i1][1].inter/tf[i1][0].bal*1200;
    td[i1].dflt:=0;
    if td[i1].py=True then
     td[i1].price:=calcprice(i1, td[i1].yld, td[i1].amt, PRECISIONFACTOR) { Calc Price for tranche}
    else td[i1].price:=td[i1].pricepct* td[i1].amt;
    td[i1].cyld:=calcyield(i1, td[i1].price +
      td[i1].amt* td[i1].coupon/1200*daysaccrued/30);{Calc Yield for each tranche}
    td[i1].dur:=calcduration(i1, td[i1].cyld); {Calculate Duration for tranche}
    td[i1].mdur:=td[i1].dur/(td[i1].cyld/1200+1);
    td[i1].avglife:=calcavglife(i1);
td[i1].fmat:=calcfmat(i1);
td[i1].lmat:=calclmat(i1);
td[i1].dflt:=calcdflt(i1);
    if td[i1].amt > 0 then td[i1].dtkdwn := td[i1].amt* td[i1].tkdwn / 100
      Else td[i1].dtkdwn := td[i1].price* td[i1].tkdwn / 100;
  end;
  SaveTrancheData;
end;


Procedure CalcNetProfitability();

Var
{year, month, day: word;}
tbondprin, tbaccrued, tunderwritingd, tbondprice: double;
i: integer;

begin
  tbondprin:=tf[0, 0].bal;
  tbaccrued:=0;
  tunderwritingd:=0;
  tbondprice:=0;
  For i:=1 to NumberOfTranches do
  begin
    tbondprice := tbondprice+td[i].price;
    tbaccrued := tbaccrued+td[i].amt* td[i].coupon / 1200 / 30 * daysaccrued;
tunderwritingd := tunderwritingd+td[i].dtkdwn;
  end;
  with StructureForm do
  begin
    Label6.Caption:=FloatToStrF(tbondprin, ffCurrency,15,2);
Label16.Caption:=FloatToStrF(tbondprice-tbondprin, ffCurrency,15,2);
Label17.Caption:=FloatToStrF(tbondprice, ffCurrency,15,2);
Label18.Caption:=FloatToStrF(tbaccrued, ffCurrency,15,2);
Label19.Caption:=FloatToStrF(tunderwritingd, ffCurrency,15,2);
Label20.Caption:=FloatToStrF(tbondprice+tbaccrued-tunderwritingd, ffCurrency,15,2);
    if cf[0].bal>0 then
    begin
     Label21.Caption:=PriceTo32nds(tbondprin/cf[0].bal*100);
Label22.Caption:=PriceTo32nds((tbondprin-tbondprice)/cf[0].bal*100);
     Label23.Caption:=PriceTo32nds(tbondprice/cf[0].bal*100);
Label24.Caption:=PriceTo32nds(tbaccrued/cf[0].bal*100);
Label25.Caption:=PriceTo32nds(tunderwritingd/cf[0].bal*100);
Label26.Caption:=PriceTo32nds((tbondprice+tbaccrued-tunderwritingd)/cf[0].bal*100);
    end
    else
    begin
     Label21.Caption:='N/A';
Label22.Caption:='N/A';
     Label23.Caption:='N/A';
     Label24.Caption:='N/A';
     Label25.Caption:='N/A';
     Label26.Caption:='N/A';
    end;
    Label27.Caption:=FloatToStrF(CollateralCost, ffCurrency,15,2);
Label34.Caption:=FloatToStrF(CollateralAccrued, ffCurrency,15,2);
Label35.Caption:=FloatToStrF(reserve[0], ffCurrency,15,2);
Label36.Caption:=FloatToStrF(StrToFloat(StructureForm.InitialIssuanceFees.Text),ffCurrency,15,2);
    Label37.Caption:=FloatToStrF(CollateralCost+CollateralAccrued+reserve[0]
     +StrToFloat(StructureForm.InitialIssuanceFees.Text),ffCurrency,15,2);
    Label38.Caption:=FloatToStrF(tbondprice+tbaccrued-tunderwritingd-(CollateralCost+CollateralAccrued+reserve[0]
     +StrToFloat(StructureForm.InitialIssuanceFees.Text)),ffCurrency,15,2);
    if cf[0].bal>0 then
    begin
     Label39.Caption:=PriceTo32nds(CollateralCost/cf[0].bal*100);
Label40.Caption:=PriceTo32nds(CollateralAccrued/cf[0].bal*100);
Label41.Caption:=PriceTo32nds(reserve[0]/cf[0].bal*100);
Label42.Caption:=PriceTo32nds(StrToFloat(StructureForm.InitialIssuanceFees.Text)/cf[0].bal*100);
     Label43.Caption:=PriceTo32nds((collateralCost+CollateralAccrued+reserve[0]
      +StrToFloat(StructureForm.InitialIssuanceFees.Text))/cf[0].bal*100);
     Label44.Caption:=PriceTo32nds((tbondprice+tbaccrued-tunderwritingd-(CollateralCost+CollateralAccrued+reserve[0]
      +StrToFloat(StructureForm.InitialIssuanceFees.Text)))/cf[0].bal*100);
    end
    else
    begin
     Label39.Caption:='N/A';
Label40.Caption:='N/A';
     Label41.Caption:='N/A';
     Label42.Caption:='N/A';
     Label43.Caption:='N/A';
     Label44.Caption:='N/A';
    end;
  end;
end; {Procedure TStructureForm.CalcClick}

function calcprice(i1:integer; yield1,amount,precision:double):double;

Var
  n1:integer;
  yield, accruedint, price:double;

begin
  price:=0;
  if yield1>=0 then
  begin
    yield := (power(yield1/200+1,1/6.0)-1)*1200;
    accruedint := daysaccrued/360.0* td[i1].coupon/100* amount;
    for n1:=1 to MAXPERIODS do
    price := price+(tf[i1][n1].prin+tf[i1][n1].inter)/power(1+yield/1200, days1stprd/30.0+n1-1);
price := price-accruedint;
    if td[i1].amt >0 then calcprice := max(int(price / amount /precision)* precision* amount,0.0)
    else calcprice := int(price*100+0.5)/100; //max(   ,0.0)
  end
  else calcprice := 0;
end;

function calcyield(i1:integer; proceeds1:double):double;

Var
TrialPrice, OrDisc, DiscRate, CDisc, Durtn, PmtDelay, ClosingDelay,
  DelayYield, fofy, fprimey, dval, guess, TargetPrice: Double;
period, counter: integer;

begin
  TargetPrice :=proceeds1;
  ClosingDelay:=0;
  guess:=0.07/12;
  PmtDelay:=days1stprd/30;
  dval:=1-PmtDelay;
  counter:=50;
  calcyield:=-999;
  while counter > 0 do
  begin
    if proceeds1< 0.0001 Then Break;
    counter := counter -1;
    OrDisc:=1+guess;
    DiscRate:=OrDisc;
    DelayYield:=my_power((1+guess),PmtDelay-ClosingDelay);
    Durtn:=0;
    TrialPrice:=0;
    for period:=1 to MAXPERIODS do
    begin
      if DiscRate<>0 then
      CDisc:=tf[i1][period].cash/DiscRate
      else CDisc:=0;
      TrialPrice:=TrialPrice+CDisc;
      Durtn:=Durtn-CDisc* period;
DiscRate:=DiscRate* OrDisc;
end;
    fofy:=(TrialPrice*(OrDisc/DelayYield))-TargetPrice;
    If(Abs(fofy)<=0.00005) Then
   begin
      calcyield:=(power(1+guess,6)-1)*200;
      Break;
    end
    else if Abs(fofy)>100000000 then Break;
fprimey:=(Durtn/DelayYield)+TrialPrice*(dval/(delayYield));
    if fprimey=0 then Break
    else guess:=guess-(fofy/fprimey);
  end;
end;

function calcduration(i1:integer; yield1:double):double;

Var
n2: integer;
myield, pv, tpv, duration: double ;

begin
  if yield1=-999 then calcduration:=-999
  else
  begin
    tpv:=0;
    duration:=0;
    myield := (power(yield1/200+1,1/6.0)-1)*1200;
    for n2:=1 to MAXPERIODS do
    begin
      pv := tf[i1, n2].cash/power(1+myield/1200, days1stprd/30.0+n2-1);
tpv := tpv+pv;
      duration := duration+pv* (days1stprd/30.0+n2-1);
    end;
    if tpv=0 then calcduration:=-999
    else calcduration:=duration / tpv / 12;
  end;
end;


function LoadTrancheData():integer;
Var
i1:integer;
begin
  If StructureForm.PrecisionComboBox.ItemIndex=1 then PrecisionFactor := 1/100000
   else if StructureForm.PrecisionComboBox.ItemIndex=2 then PrecisionFactor := 1/3200
   else if StructureForm.PrecisionComboBox.ItemIndex=3 then PrecisionFactor := 1/6400
   else PrecisionFactor:= 1/10000000;

  with StructureForm.StructureTable do
  begin
    DisableControls;
First;
    i1:=0;
    SeniorFlag:=False;
    fillchar(td,sizeof(td),0);
    InitialTotBonds:=0;
    while Not(EOF) do
    begin
      i1:=i1+1;
      td[i1].origamt:=FieldByName('Balance').AsFloat;
      InitialTotBonds:=InitialTotBonds+td[i1].origamt;
      if StructureForm.UpdateComboBox.ItemIndex<1 then
      begin
        td[i1].amt:=FieldByName('Balance').AsFloat;
        td[i1].pricepct:=ReadPrecision(FieldByName('Price %').AsString,PrecisionFactor);
      end
      else
      begin
        td[i1].amt:=FieldByName('Updated Balance').AsFloat;
        td[i1].pricepct:=ReadPrecision(FieldByName('Updated Price %').AsString,PrecisionFactor);
      end;
      td[i1].name:=FieldByName('Class').AsString;
      td[i1].coupon:=FieldByName('Coupon').AsFloat;
      td[i1].yld:=FieldByName('Tsy Yield').AsFloat+FieldByName('Spread').AsFloat/100;
      td[i1].spread:=FieldByName('Spread').AsFloat/100;
      td[i1].tkdwn:=FieldByName('Tkdwn %').AsFloat;
      td[i1].ttype:=Uppercase(FieldByName('Type').AsString);
      td[i1].PAC:=FieldByName('PAC').AsInteger;
      td[i1].serial:=(Uppercase(FieldByName('Type').AsString)='S');
      td[i1].zbond:=(Uppercase(FieldByName('Z').AsString)='Z');
      td[i1].py:=(Uppercase(FieldByName('P/Y').AsString)='Y');
      td[i1].price:=FieldByName('Price $').AsFloat;
      td[i1].Sub:=(Uppercase(FieldByName('S').AsString)='J');
      td[i1].Mez:=(Uppercase(FieldByName('S').AsString)='M');
      td[i1].io:=(Uppercase(FieldByName('I').AsString)='I');  // Interest Only Tranche
      td[i1].wac:=(Uppercase(FieldByName('W').AsString)='W'); // WAC Coupon Tranche - Right Click for offset
      td[i1].cwac:=FieldByName('CWAC%').AsFloat;
      td[i1].swac:=FieldByName('SWAC%').AsFloat;
      td[i1].kwac:=FieldByName('KWAC%').AsFloat;
      td[i1].fserial:=FieldByName('FSERIAL').AsInteger;
      td[i1].lserial:=FieldByName('LSERIAL').AsInteger;
      if (td[i1].Sub or td[i1].Mez) then SeniorFlag:=True;
      if td[i1].ttype='SM' then // Simultaneous Maturity - right click for parallel tranches
      begin
        CalcLikeArray(FieldByName('PT1').AsInteger,FieldByName('PP1').AsFloat,i1);
        CalcLikeArray(FieldByName('PT2').AsInteger,FieldByName('PP2').AsFloat,i1);
        CalcLikeArray(FieldByName('PT3').AsInteger,FieldByName('PP3').AsFloat,i1);
        CalcLikeArray(FieldByName('PT4').AsInteger,FieldByName('PP4').AsFloat,i1);
        CalcLikeArray(FieldByName('PT5').AsInteger,FieldByName('PP5').AsFloat,i1);
        CalcLikeArray(FieldByName('PT6').AsInteger,FieldByName('PP6').AsFloat,i1);
        CalcLikeArray(FieldByName('PT7').AsInteger,FieldByName('PP7').AsFloat,i1);
        CalcLikeArray(FieldByName('PT8').AsInteger,FieldByName('PP8').AsFloat,i1);
        CalcLikeArray(FieldByName('PT9').AsInteger,FieldByName('PP9').AsFloat,i1);
        CalcLikeArray(FieldByName('PT10').AsInteger,FieldByName('PP10').AsFloat,i1);
      end;
      Next;
    end;
    EnableControls;
  end;
  LoadTrancheData:=i1;
end;

Function ReadPrecision(pricestring:string; precision:double):double;
var
price, onesixtyfourth:double;
begin
  if pos('-', PriceString)>0 then   // Price quoted in 32nds
   begin
    onesixtyfourth:=0;
    if pos('+', PriceString)>0 then
     begin
      onesixtyfourth:=1/64;
      PriceString[pos('+', PriceString)]:=' ';
    end;
    DecimalSeparator:='-';
    price:=StrToFloat(pricestring);
DecimalSeparator:='.';
    ReadPrecision:=(int(price)+(price-int(price))*100/32+onesixtyfourth)/100;
  end
  else                             // Price quoted in decimal form
  begin
    if length(pricestring)>0 then
     price:=StrToFloat(pricestring)
     else price:=0;
    ReadPrecision:=price/100;
  end;
end;

Function WritePrecision(price, precision:double):string;
var
p:integer;
PriceString:String;
fraction:double;
begin
  p:=StructureForm.PrecisionComboBox.ItemIndex;
  if p>1 then
  begin
    fraction:=price-int(price);
    DecimalSeparator:='-';
    PriceString:=Format('%10.2f', [int(price)+int(fraction*32)/100]);
    DecimalSeparator:='.';
    if (p=3) and(int(fraction*64)-int(fraction*32)*2=1) then PriceString:=PriceString+'+'
    else PriceString:=PriceString+' ';
  end
  else if p=1 then PriceString:=Format('%10.3f', [price])
  else PriceString:=Format('%10.5f', [price]);
WritePrecision:=PriceString;
end;

Procedure CalcLikeArray(i:integer; pct:double;i1:integer);
var
j:integer;
// i1 is the current tranche, i is the parallel tranche
begin
  if (i>0) and not(td[i].io or td[i1].io) then  // Can't pay parallel to an IO, add error message
  begin
    j:=td[i].like[0].tranche+1;   // Add error language if j>10 too many parallel tranches
    if j<=MAXLIKETRANCHES then
    begin
      td[i].like[0].tranche:=j;
      td[i].like[0].amt:=td[i].like[0].amt+td[i1].origamt* pct/100;
      td[i].like[j].tranche:=i1;
      td[i].like[j].amt:=td[i1].origamt* pct/100;
    end;
  end else
  if (i>0) and td[i1].io then
  begin
    j:=td[i1].like[0].tranche+1;   // Add error language if j>10 too many parallel tranches
    if j<=MAXLIKETRANCHES then
    begin
      td[i1].like[0].tranche:=j;
      td[i1].like[0].amt:=td[i1].like[0].amt+pct;
      td[i1].like[j].tranche:=i;  // i1 is the current tranche, i is the parallel tranche
      td[i1].like[j].amt:=pct;
    end;
  end;
end;

Procedure GetLocation();
begin
  with StructureForm.StructureTable do
  begin
    DisableControls;
CurrentTranche:=0;
    while not(BOF) do
    begin
      CurrentTranche:=CurrentTranche+1;
      Prior;
    end;
    EnableControls;
  end;
end;

Procedure GotoLocation();
var
n:integer;
begin
  with StructureForm.StructureTable do
  begin
    DisableControls;
n:=1;
    First;
    while n<CurrentTranche do
    begin
      Next;
      n:=n+1;
    end;
    EnableControls;
  end;
end;

Procedure SaveTrancheData();

Var
i:integer;
begin
  i:=0;
  with StructureForm.StructureTable do
  begin
    DisableControls;
First;
    while Not(EOF) do
    begin
      i:=i+1;
      Edit;
      if td[i].io then FieldByName('Coupon').AsFloat:=td[i].coupon;
      if td[i].amt>0 then
        if StructureForm.UpdateComboBox.ItemIndex<1 then
          FieldByName('Price %').AsString:=WritePrecision(td[i].price/td[i].amt*100, PrecisionFactor)
        else FieldByName('Updated Price %').AsString:=WritePrecision(td[i].price/td[i].amt*100, PrecisionFactor)
      else
        if StructureForm.UpdateComboBox.ItemIndex<1 then
          FieldByName('Price %').AsString:=WritePrecision(0, PrecisionFactor)
        else FieldByName('Updated Price %').AsString:=WritePrecision(0, PrecisionFactor);
      FieldByName('Avg. Life').AsFloat:=td[i].avglife;
      FieldByName('CBEY').AsFloat:=td[i].cyld;
      FieldByName('MDurat').AsFloat:=td[i].mdur;
      FieldByName('Durat').AsFloat:=td[i].dur;
      FieldByName('Price $').AsFloat:=td[i].price;
      if td[i].dflt=1 then FieldByName('F').AsString:='D'
        Else FieldByName('F').AsString:='';
      FieldByName('Accrued').AsFloat:=td[i].amt* td[i].coupon/1200* daysaccrued/30;
      FieldByName('Proceeds').AsFloat:=td[i].price+td[i].amt* td[i].coupon/1200* daysaccrued/30;
      FieldByName('Tkdwn $').AsFloat:=td[i].amt* td[i].tkdwn/100;
      FieldByName('Type').AsString:=td[i].ttype;
      FieldByName('PAC').AsFloat:=td[i].PAC;
      If td[i].amt>0 then FieldByName('Window').AsString:=
      MonthToDate(td[i].fmat) + ' - ' + MonthToDate(td[i].lmat)
      else FieldByName('Window').AsString:='N/A';
      Next;
    end;
    EnableControls;
  end;
end;

function MonthToDate(m:integer):String;
var
year, month, day: word;
begin
  if StructureForm.UpdateComboBox.ItemIndex<1 then
    DecodeDate(StrToDate(StructureForm.FirstPaymentDate.Text),year,month,day)
    else
    DecodeDate(StrToDate(StructureForm.FirstPaymentDateUpdate.Text),year,month,day);
  MonthToDate:=FormatDateTime('mm/yy', EncodeDate(Year+(Month+m-2)
   div 12,(Month+m-2)mod 12+1,Day));
end;

function PriceTo32nds(const price1:double):string;

begin
  DecimalSeparator:='-';
  PriceTo32nds:=Format('%7.2f', [int(price1)+int((price1-int(price1))*32)/100]);
  DecimalSeparator:='.';
end;

function calcavglife(i1:integer):double;

var
n:integer;
amount, bondyears:double;

begin
  bondyears:=0;
  amount:=0;
  for n:=1 to MAXPERIODS do
  begin
    bondyears:=bondyears+max(tf[i1][n].prin,0)*(n-1+days1stprd/30.0);
    amount:=amount+max(tf[i1][n].prin,0);
end;
  if amount > 0 then
  calcavglife := bondyears / amount / 12
  Else calcavglife := 0;
end;


function calcfmat(i1:integer):integer;
var
n, result1:integer;
begin
  result1:=MAXPERIODS;
  for n:=1 to MAXPERIODS do
  if((tf[i1][n].prin>0)and(tf[i1][n - 1].bal= tf[i1, 0].bal)) then result1:=n;
  calcfmat:=result1;
end;

function calclmat(i1:integer):integer;
var
n, result2:integer;

begin
  result2:=MAXPERIODS;
  for n:=1 to MAXPERIODS do
  if((tf[i1][n].prin>0)and(tf[i1][n].bal= 0)) then result2:=n;
  calclmat:=result2;
end;

function calcdflt(i1:integer):integer;
var
n:integer;
begin
  calcdflt:=0;
  if tf[i1][MAXPERIODS].bal>0.001 then calcdflt:=1;
  for n:=1 to MAXPERIODS do
   if(tf[i1][n].inter<(tf[i1][n - 1].bal* td[i1].coupon/1200-0.001)) then calcdflt:=1;
end;

Procedure LoadCollateralCashFlows();
Var
n:integer;
day,year,month:word;
begin
  cf[0].uprin:=0;
  cf[0].sprin:=0;
  cf[0].inter:=0;
  cf[0].serv:=0;
  if StructureForm.UpdateComboBox.ItemIndex=1 then
  DecodeDate(StrToDate(StructureForm.FirstPaymentDateUpdate.Text),year,month,day)
  else DecodeDate(StrToDate(StructureForm.FirstPaymentDate.Text),year,month,day);
  with ViewCollCashFlowsForm.CollCashFlowsGrid do
  begin
    for n:=1 to MAXPERIODS do
    begin
      Cells[0, n+1]:=Format('%3d', [(n)]);
      if day<29 then Cells[0, n + 1]:= Cells[0, n + 1]+': '+
      Format('%8s',[DateToStr(EncodeDate(Year + (Month + n - 2)div 12, ((Month + n - 2)mod 12) + 1, Day))]);
      Cells[1, n + 1]:=Format('%18.2n', [cf[n].bal]);
      Cells[2, n + 1]:=Format('%18.2n', [cf[n].sprin]);
      Cells[3, n + 1]:=Format('%18.2n', [cf[n].uprin]);
      Cells[4, n + 1]:=Format('%18.2n', [cf[n].sprin+cf[n].uprin]);
      Cells[5, n + 1]:=Format('%18.2n', [cf[n].inter]);
      Cells[6, n + 1]:=Format('%18.2n', [cf[n].sprin+
      cf[n].uprin+cf[n].inter]);
      cf[0].uprin:=cf[0].uprin+cf[n].uprin;
      cf[0].sprin:=cf[0].sprin+cf[n].sprin;
      cf[0].inter:=cf[0].inter+cf[n].inter;
      cf[0].serv:=cf[0].serv+cf[n].serv;
    end;
    Cells[0, 0]:='';
    Cells[1, 0]:=Format('%18.2m', [cf[0].bal]);
    Cells[2, 0]:=Format('%18.2m', [cf[0].sprin]);
    Cells[3, 0]:=Format('%18.2m', [cf[0].uprin]);
    Cells[4, 0]:=Format('%18.2m', [cf[0].uprin+cf[0].sprin]);
    Cells[5, 0]:=Format('%18.2m', [cf[0].inter]);
    Cells[6, 0]:=Format('%18.2m', [cf[0].sprin+
    cf[0].uprin+cf[0].inter]);
    Cells[0, 1]:='Prd';
    Cells[1, 1]:='       Balance';
    Cells[2, 1]:='   Sched Principal';
    Cells[3, 1]:=' Unsched Principal';
    Cells[4, 1]:='   Total Principal';
    Cells[5, 1]:='      Interest';
    Cells[6, 1]:='        Total';
  end;
end;

Procedure LoadTrancheCashFlows();
Var
i, n:integer;
day,year,month:word;
begin
  if StructureForm.UpdateComboBox.ItemIndex=1 then
  DecodeDate(StrToDate(StructureForm.FirstPaymentDateUpdate.Text),year,month,day)
  else DecodeDate(StrToDate(StructureForm.FirstPaymentDate.Text),year,month,day);
  with ViewTrancheCashFlowsForm.TrancheCashFlowsGrid do
  begin
    ColCount:=NumberOfTranches*4+1;
    for n:=1 to MAXPERIODS do
    begin
      Cells[0, n + 1]:=Format('%3d', [(n)]);
      if day<29 then Cells[0, n + 1]:= Cells[0, n + 1]+'  '+
      Format('%8s',[DateToStr(EncodeDate(Year + (Month + n - 2)div 12, ((Month + n - 2)mod 12) + 1, Day))]);
      for i:=1 to NumberOfTranches do
      begin
        Cells[(i - 1) * 4 + 1, n + 1]:=Format('%18.2n', [tf[i, n].bal]);
        Cells[(i - 1) * 4 + 2, n + 1]:=Format('%18.2n', [tf[i, n].prin]);
        Cells[(i - 1) * 4 + 3, n + 1]:=Format('%18.2n', [tf[i, n].inter]);
        Cells[(i - 1) * 4 + 4, n + 1]:=Format('%18.2n', [tf[i, n].cash]);
      end;
    end;
    Cells[0, 0]:=StructureForm.ModelSpeed.Text+' ';
    If StructureForm.Model.ItemIndex<1 then Cells[0,0]:=Cells[0, 0]+'PSA' else
    begin
      if StructureForm.Model.ItemIndex= 1 then Cells[0,0]:=Cells[0, 0]+'CPR' else
      Cells[0, 0]:=Cells[0, 0]+'MDL';
end;
    Cells[0, 1]:='Prd';
    for i:=1 to NumberOfTranches do
    begin
      Cells[(i - 1) * 4 + 1, 0]:=Format('%18.2m', [tf[i,0].bal]);
      Cells[(i - 1) * 4 + 2, 0]:=Format('%18.2m', [tf[i,0].prin]);
      Cells[(i - 1) * 4 + 3, 0]:=Format('%18.2m', [tf[i,0].inter]);
      Cells[(i - 1) * 4 + 4, 0]:=Format('%18.2m', [tf[i,0].cash]);
      Cells[(i - 1) * 4 + 1, 1]:='    '+td[i].name + ' Balance # '+IntToStr(i);
Cells[(i - 1) * 4 + 2, 1]:='  '+td[i].name + ' Principal # '+IntToStr(i);
Cells[(i - 1) * 4 + 3, 1]:='   '+td[i].name + ' Interest # '+IntToStr(i);
Cells[(i - 1) * 4 + 4, 1]:='       '+td[i].name + ' Cash # '+IntToStr(i);
end;
  end;
end;

Procedure LoadExpensesCashFlows();
Var
n:integer;
day,year,month:word;
begin
  expenses[0]:=0;
  residual[0]:=0;
  if StructureForm.UpdateComboBox.ItemIndex=1 then
  DecodeDate(StrToDate(StructureForm.FirstPaymentDateUpdate.Text),year,month,day)
  else DecodeDate(StrToDate(StructureForm.FirstPaymentDate.Text),year,month,day);
  with ViewExpensesCashFlowsForm.ExpensesCashFlowsGrid do
  begin
    ColCount:=10;
    for n:=1 to MAXPERIODS do
    begin
      Cells[0, n + 1]:=Format('%3d', [(n)]);
      if day<29 then Cells[0, n + 1]:= Cells[0, n + 1]+': '+
      Format('%8s',[DateToStr(EncodeDate(Year + (Month + n - 2)div 12, ((Month + n - 2)mod 12) + 1, Day))]);

      expenses[0]:=expenses[0]+expenses[n];
      residual[0]:=residual[0]+residual[n];

      Cells[1, n + 1]:=Format('%18.2n', [cf[n].uprin+cf[n].sprin]);
      Cells[2, n + 1]:=Format('%18.2n', [cf[n].inter]);
      Cells[3, n + 1]:=Format('%18.2n', [cf[n].uprin+cf[n].sprin+cf[n].inter]);
      Cells[4, n + 1]:=Format('%18.2n', [expenses[n]]);
      Cells[5, n + 1]:=Format('%18.2n', [tf[0, n].bal]);
      Cells[6, n + 1]:=Format('%18.2n', [tf[0, n].prin]);
      Cells[7, n + 1]:=Format('%18.2n', [tf[0, n].inter]);
      Cells[8, n + 1]:=Format('%18.2n', [residual[n]]);
      Cells[9, n + 1]:=Format('%18.2n', [tf[0, n].inter+residual[n]]);

    end;

    Cells[0, 0]:='';
    Cells[1, 0]:=Format('%18.2m', [cf[0].uprin+cf[0].sprin]);
    Cells[2, 0]:=Format('%18.2m', [cf[0].inter]);
    Cells[3, 0]:=Format('%18.2m', [cf[0].uprin+cf[0].sprin+cf[0].inter]);
    Cells[4, 0]:=Format('%18.2m', [expenses[0]]);
    Cells[5, 0]:=Format('%18.2m', [tf[0,0].bal]);
    Cells[6, 0]:=Format('%18.2m', [tf[0,0].prin]);
    Cells[7, 0]:=Format('%18.2m', [tf[0,0].inter]);
    Cells[8, 0]:=Format('%18.2m', [residual[0]]);
    Cells[9, 0]:=Format('%18.2m', [tf[0,0].inter+residual[0]]);

    Cells[0, 1]:='Prd';
    Cells[1, 1]:=' Principal Income';
    Cells[2, 1]:='  Interest Income';
    Cells[3, 1]:='     Total Income';
    Cells[4, 1]:='         Expenses';
    Cells[5, 1]:='    Bonds Balance';
    Cells[6, 1]:='  Bonds Principal';
    Cells[7, 1]:='   Bonds Interest';
    Cells[8, 1]:='      Residual CF';
    Cells[9, 1]:='   Total Interest';
  end;
end;

Procedure LoadIncomeCashFlows();
Var
n:integer;
day,year,month:word;
begin
  ReinvIncome[0]:=0;
  if StructureForm.UpdateComboBox.ItemIndex=1 then
  DecodeDate(StrToDate(StructureForm.FirstPaymentDateUpdate.Text),year,month,day)
  else DecodeDate(StrToDate(StructureForm.FirstPaymentDate.Text),year,month,day);
  with ViewIncomeCashFlowsForm.IncomeCashFlowsGrid do
  begin
    ColCount:=14;
    for n:=1 to MAXPERIODS do
    begin
      Cells[0, n + 1]:=Format('%3d', [(n)]);
      if day<29 then Cells[0, n + 1]:= Cells[0, n + 1]+': '+
      Format('%8s',[DateToStr(EncodeDate(Year + (Month + n - 2)div 12, ((Month + n - 2)mod 12) + 1, Day))]);

      ReinvIncome[0]:=ReinvIncome[0]+ReinvIncome[n];

      Cells[1, n + 1]:=Format('%18.2n', [cf[n].bal]);
      Cells[2, n + 1]:=Format('%18.2n', [cf[n].inter]);
      Cells[3, n + 1]:=Format('%18.2n', [cf[n].serv]);
      Cells[4, n + 1]:=Format('%18.2n', [cf[n].sprin]);
      Cells[5, n + 1]:=Format('%18.2n', [cf[n].uprin]);
      Cells[6, n + 1]:=Format('%18.2n', [cf[n].sprin+cf[n].uprin]);
      Cells[7, n + 1]:=Format('%18.2n', [SeniorPrin[n]]);
      Cells[8, n + 1]:=Format('%18.2n', [MezExtraPrin[n]]);
      Cells[9, n + 1]:=Format('%18.2n', [JuniorPrin[n]]);
      Cells[10, n + 1]:=Format('%18.2n', [ReinvIncome[n]]);
      Cells[11, n + 1]:=Format('%18.2n', [cf[n].inter+cf[n].sprin+cf[n].uprin+ReinvIncome[n]]);
      Cells[12, n + 1]:=Format('%18.2n', [reserve[n-1]-reserve[n]]);
      Cells[13, n + 1]:=Format('%18.2n', [reserve[n]]);



    end;

    Cells[0, 0]:='';
    Cells[1, 0]:=Format('%18.2n', [cf[0].bal]);
    Cells[2, 0]:=Format('%18.2n', [cf[0].inter]);
    Cells[3, 0]:=Format('%18.2n', [cf[0].serv]);
    Cells[4, 0]:=Format('%18.2n', [cf[0].sprin]);
    Cells[5, 0]:=Format('%18.2n', [cf[0].uprin]);
    Cells[6, 0]:=Format('%18.2n', [cf[0].sprin+cf[0].uprin]);
    Cells[7, 0]:=Format('%18.2n', [SeniorPrin[0]]);
    Cells[8, 0]:=Format('%18.2n', [MezExtraPrin[0]]);
    Cells[9, 0]:=Format('%18.2n', [JuniorPrin[0]]);
    Cells[10, 0]:=Format('%18.2n', [ReinvIncome[0]]);
    Cells[11, 0]:=Format('%18.2n', [cf[0].inter+cf[0].sprin+cf[0].uprin+ReinvIncome[0]]);
    Cells[12, 0]:=Format('%18.2n', [0.0]);
Cells[13, 0]:=Format('%18.2n', [reserve[0]]);

    Cells[0, 1]:='Prd';
    Cells[1, 1]:='Remaining Balance';
    Cells[2, 1]:='         Interest';
    Cells[3, 1]:='        Servicing';
    Cells[4, 1]:='  Sched Principal';
    Cells[5, 1]:='Unsched Principal';
    Cells[6, 1]:='  Total Principal';
    Cells[7, 1]:=' Senior Principal';
    Cells[8, 1]:='   Mezz Principal';
    Cells[9, 1]:=' Junior Principal';
    Cells[10, 1]:='    Reinv Income';
    Cells[11, 1]:=' Total Cash Flow';
    Cells[12, 1]:='Resv Fnd Outflow';
    Cells[13, 1]:='Resv Fnd Balance';


  end;
end;

function CalcTsyRate(avglife:double; mode:integer):double;
type
TsyArrayType = Array[1..2, 1..9] of Double;
var
high, low :integer;
A:TsyArrayType;
begin
  with EconomyForm do
  begin
    A[1, 1]:=0.5;
    A[2, 1]:=StrToFloat(Edit1.text);
A[1, 2]:=1;
    A[2, 2]:=StrToFloat(Edit2.text);
A[1, 3]:=2;
    A[2, 3]:=StrToFloat(Edit3.text);
A[1, 4]:=3;
    A[2, 4]:=StrToFloat(Edit4.text);
A[1, 5]:=5;
    A[2, 5]:=StrToFloat(Edit5.text);
A[1, 6]:=10;
    A[2, 6]:=StrToFloat(Edit6.text);
A[1, 7]:=15;
    A[2, 7]:=StrToFloat(Edit7.text);
A[1, 8]:=20;
    A[2, 8]:=StrToFloat(Edit8.text);
A[1, 9]:=30;
    A[2, 9]:=StrToFloat(Edit9.text);
end;
  low:=9;
  high:=1;
  while (avglife<A[1, low]) and (low>1) do low:=low-1;
  while (avglife>A[1, high]) and(high<9) do high:=high+1;
  CalcTsyRate:=A[2, low];
  if mode=2 then CalcTsyRate:=A[2, high];
  if (mode<1) and not(high=low)then CalcTsyRate:=A[2, low]+(A[2, high]-A[2, low])/
  (A[1, high]-A[1, low])*(avglife-A[1, low]);
end;

function Days360(date1, date2:string):integer;
var
z1, z2, year1, month1, day1, year2, month2, day2: word;
begin
  DecodeDate(StrToDate(date1),year1,month1,day1);
  DecodeDate(StrToDate(date2),year2,month2,day2);
  //February and leap year adjustment.
  if (month1=2) and(year1 mod 4 = 0) and(day1= 29) then day1:=30;
  if (month1=2) and(year1 mod 4 <> 0) and(day1= 28) then day1:=30;
  if day1=31 then z1:=30 else z1:=day1;
  if day2<31 then z2:=day2 else if day1<30 then z2:=day2 else z2:=30;
  Days360:=360*(year2-year1)+30*(month2-month1)+z2-z1;

end;


Procedure CalcDates();
var
year, month, day: word;
begin
  with StructureForm do
  begin
    if UpdateComboBox.ItemIndex< 1 then
    begin
      daysaccrued:=int(StrtoDate(SettlementDate.text))-int(StrtoDate(DatedDate.text));
      days1stprd:=Days360(SettlementDate.Text, FirstPaymentDate.Text);
end
    else
    begin
      DecodeDate(StrToDate(FirstPaymentDateUpdate.Text)-StrtoFloat(DelayDays.Text),year,month,day);
      DatedDateUpdate.Text:=FormatDateTime('mm/dd/yyyy',
       EncodeDate(Year-1+(month+10)div 12,(Month+10)mod 12+1,1));
      daysaccrued:=int(StrtoDate(SettlementDateUpdate.text))-int(StrtoDate(DatedDateUpdate.text));
      days1stprd:=Days360(SettlementDateUpdate.Text, FirstPaymentDateUpdate.Text);
end;
  end;
end;

function my_power(x:double; y:double):double;
begin
  if x<0 then
  my_power:= power(abs(x),y)*(-1)
  else
  my_power:= power(abs(x),y);
end;

function LoadPACData():integer;
var
i:integer;
begin
  with PACFinderForm.PACDataTable do
  begin
    i:=0;
    DisableControls;
    First;
    while Not(EOF) do
    begin
      i:=i+1;
      pd[i].TrancheNumber:=FieldByName('No.').AsInteger;
      pd[i].amt:=FieldByName('Amount').AsFloat;
      Next
    end;
First;
    EnableControls;
    LoadPACData:=i;
  end;

end;

procedure CalcPACTableCashFlows();
var
n, i:integer;
pacprin,classprin:double;
begin
  NumberOfPACTranches:=LoadPACData;
  For i:=1 to NumberOfPACTranches do PACTableArray[i, 0].bal:=pd[i].amt;
  For n:=1 to MAXPERIODS do
  begin
    zaccrued:=0;
    for i:=1 to NumberOfPACTranches do
    if td[pd[i].TrancheNumber].zbond=true then
    begin
      zaccrued:=zaccrued+PACTableArray[i, n - 1].bal* td[pd[i].TrancheNumber].coupon/1200;
      PACTableArray[i, n].prin:= -PACTableArray[i, n - 1].bal* td[pd[i].TrancheNumber].coupon/1200;
    end;
    pacprin:=PACTableArray[0, n].prin+zaccrued;
    for i:=1 to NumberOfPACTranches do
    begin
      classprin:=Min(PACTableArray[i, n - 1].bal-PACTableArray[i, n].prin, pacprin);
pacprin:=pacprin-classprin;
      PACTableArray[i, n].prin:=PACTableArray[i, n].prin+classprin;
      PACTableArray[i, n].bal:=PACTableArray[i, n - 1].bal-PACTableArray[i, n].prin;
      // PACTableArray[0,n].bal must be zero for non PAC tranches to pay properly
    end;
  end;
end;


Procedure LoadPACCashFlowsGrid(s:string);
var
i, n:integer;
begin
  NumberOfPACTranches:=LoadPACData;
  with ViewPACCashFlowsForm.PACCashFlowsGrid do
  begin
    ColCount:=NumberOfPACTranches+1;
Cells[0, 0]:='      Period';
    for i:=1 to NumberOfPACTranches do Cells[i, 0]:= 'No. '+ IntToStr(i)+ ' Tranche '+td[pd[i].TrancheNumber].name;
    begin
      for n:=0 to MAXPERIODS do
      begin
        Cells[0, n + 1]:=IntToStr(n);
        for i:=1 to NumberOfPACTranches do if s='Balance' then
         Cells[i, n + 1]:=Format('%18.2n', [PACTableArray[i, n].bal])
        else Cells[i, n + 1]:=Format('%18.2n', [PACTableArray[i, n].prin]);
      end;
    end;
  end;
end;

Procedure LoadPACUseTable();
var
i, n:integer;
begin
  n:=0;
  with PACTableForm.PACTable do
  begin
    DisableControls;
First;
    while Not(EOF) do
    begin
      for i:=1 to MAXPACTRANCHES do
      begin
        PACUseTable[i, n]:=Fields[i + 1].AsFloat;
      end;
      Next;
      n:=n+1;
    end;
    First;
    EnableControls;
  end;
end;

Procedure AdjustTrancheCashFlows(i:integer; fserialskip,lserialskip:double);
var
n:integer;
p, zfactor, principalskip:double;
begin
  p:= int(Days360(StructureForm.SettlementDate.Text, StructureForm.SettlementDateUpdate.Text)/30.0+0.9999);

  if (StructureForm.UpdateComboBox.ItemIndex=1) and td[i].zbond then
  zfactor:= my_power( 1 + (td[i].coupon/1200),p )
  else zfactor:= 1;


  tf[i, 0].bal:=td[i].amt-(fserialskip+lserialskip)*25000* zfactor;

n:=1;
  while fserialskip>0 do
  begin
    if td[i].zbond then principalskip:=Min(Max(tf[i, n].prin,0),fserialskip*25000* zfactor* power(1+td[i].coupon/1200, n))
     else principalskip:=Min(tf[i, n].prin, fserialskip*25000);
tf[i, n].prin:=tf[i, n].prin-principalskip;
    if td[i].zbond then fserialskip:=fserialskip-principalskip/(power(1+td[i].coupon/1200, n)*25000* zfactor)
     else fserialskip:=fserialskip-principalskip/25000;
    n:=n+1;
  end;
  n:=MAXPERIODS;
  while lserialskip>0 do
  begin
    if td[i].zbond then principalskip:=Min(Max(tf[i, n].prin,0),lserialskip*25000* zfactor* power(1+td[i].coupon/1200, n))
     else principalskip:=Min(tf[i, n].prin, lserialskip*25000);
tf[i, n].prin:=tf[i, n].prin-principalskip;
    if td[i].zbond then lserialskip:=lserialskip-principalskip/(power(1+td[i].coupon/1200, n)*25000* zfactor)
     else lserialskip:=lserialskip-principalskip/25000;
    n:=n-1;
  end;
  tf[i, 0].prin:=0;
  tf[i, 0].inter:=0;
  tf[i, 0].cash:=0;
  for n:=1 to MAXPERIODS do
  begin
    tf[i, n].bal:=tf[i, n - 1].bal-tf[i, n].prin;
    tf[i, n].inter:=tf[i, n - 1].bal* td[i].coupon/1200;
    tf[i, n].cash:=tf[i, n].inter+tf[i, n].prin;

    tf[i, 0].prin:=tf[i, 0].prin+tf[i, n].prin;
    tf[i, 0].inter:=tf[i, 0].inter+tf[i, n].inter;
    tf[i, 0].cash:=tf[i, 0].cash+tf[i, n].cash;
  end;
end;

Procedure LoadPriceYieldCashFlows(i, s:integer);
Var
n:integer;
day,year,month:word;
ModelName:String;
begin
  with ViewPriceYieldCashFlowsForm.PriceYieldCashFlowsGrid do
  begin
    if s=7 then
    begin
      if StructureForm.UpdateComboBox.ItemIndex=1 then
      DecodeDate(StrToDate(StructureForm.FirstPaymentDateUpdate.Text),year,month,day)
      else DecodeDate(StrToDate(StructureForm.FirstPaymentDate.Text),year,month,day);
      Cells[0, 0]:='   '+td[i].name + ':  # '+IntToStr(i);
Cells[0, 1]:='Prd';
      for n:=1 to MAXPERIODS do
      begin
        Cells[0, n + 1]:=Format('%3d', [(n)]);
        if day<29 then Cells[0, n + 1]:= Cells[0, n + 1]+': '+
        Format('%8s',[DateToStr(EncodeDate(Year + (Month + n - 2)div 12, ((Month + n - 2)mod 12) + 1, Day))]);
      end;
    end;
    for n:=1 to MAXPERIODS do
    begin
      Cells[(s - 1) * 4 + 1, n + 1]:=Format('%18.2n', [tf[i, n].bal]);
      Cells[(s - 1) * 4 + 2, n + 1]:=Format('%18.2n', [tf[i, n].prin]);
      Cells[(s - 1) * 4 + 3, n + 1]:=Format('%18.2n', [tf[i, n].inter]);
      Cells[(s - 1) * 4 + 4, n + 1]:=Format('%18.2n', [tf[i, n].cash]);
    end;
    If ModelArray[s]<1 then ModelName:='PSA' else
    begin
      if ModelArray[s]=1 then ModelName:='CPR'else ModelName:='MDL';
    end;
    Cells[(s - 1) * 4 + 1, 0]:=Format('%18.2m', [tf[i,0].bal]);
    Cells[(s - 1) * 4 + 2, 0]:=Format('%18.2m', [tf[i,0].prin]);
    Cells[(s - 1) * 4 + 3, 0]:=Format('%18.2m', [tf[i,0].inter]);
    Cells[(s - 1) * 4 + 4, 0]:=Format('%18.2m', [tf[i,0].cash]);
    Cells[(s - 1) * 4 + 1, 1]:='  '+Format('%7.2n', [SpeedArray[s]]) +' '+ModelName+ ' Bal';
    Cells[(s - 1) * 4 + 2, 1]:='  ' + ' Principal' ;
    Cells[(s - 1) * 4 + 3, 1]:='   ' + ' Interest' ;
    Cells[(s - 1) * 4 + 4, 1]:='       ' + ' Cash' ;
  end;
end;

function Max(A:Double; B:Double):Double;
begin
  If A>B Then Max:=A Else Max:=B;
end;

function Min(A:Double; B:Double):Double;
begin
  If A<B Then Min:=A Else Min:=B;
end;

Procedure CalcStructureForm();
begin
  GetLocation();
  CalcCollateralCashFlows(StrtoFloat(StructureForm.ModelSpeed.Text),StructureForm.Model.ItemIndex);
  CalcTrancheCashFlows();
  CalcTrancheStatistics();
  CalcNetProfitability();
  LoadCollateralCashFlows();
  LoadTrancheCashFlows();
  LoadExpensesCashFlows();
  LoadIncomeCashFlows();
  GotoLocation();
end;

function CalcCollCertExcess():Double;
var
StructureBalance, CollBalance: double;
begin
with StructureForm.StructureTable do
 begin
 DisableControls;
First;
 StructureBalance:=0;
  while Not(EOF) do
   begin
   Edit;
StructureBalance:=StructureBalance+FieldByName('Balance').AsFloat;
   Next;
   End;
  First;
  EnableControls;
 end;
with CollateralForm.CollateralTable do
 begin
 DisableControls;
First;
 CollBalance:=0;
 while Not(EOF) do
  begin
  CollBalance := CollBalance + FieldByName('CalcBal').AsFloat;
  Next;
  end;
 First;
 EnableControls;
 end;
CalcCollCertExcess:=CollBalance-StructureBalance;
end;


end.
*/
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
*/
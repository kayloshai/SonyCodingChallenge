using System;
using System.Collections.Generic;

namespace SonyCodingChallenge
{
    public class MachineInfo {
        private string MachineInfoName;
        internal MachineInfo(string from) {
            this.MachineInfoName = from;
        }
        public string MachineInfoNumber{
            get { return this.MachineInfoName; }
        }
    }

    public class SubjectHandler : IObservable<MachineInfo>{
        private List<IObserver<MachineInfo>> observers;
        private List<MachineInfo> MachineInfos;

        public SubjectHandler(){
            observers = new List<IObserver<MachineInfo>>();
            MachineInfos = new List<MachineInfo>();
        }

        public IDisposable Subscribe(IObserver<MachineInfo> observer){
            // Check whether observer is already registered. If not, add it
            if (!observers.Contains(observer)){
                observers.Add(observer);
                // Provide observer with existing data.
                foreach (var item in MachineInfos)
                    observer.OnNext(item);
            }
            return new Unsubscriber<MachineInfo>(observers, observer);
        }

        //instead of machineinfoname, provide 1 to continue random input or 0 to stop
        public void MachineInfoStatus(string  _string){
            var info = new MachineInfo(_string);

            // MachineInfoName is assigned, so add new info object to list.
            if (!MachineInfos.Contains(info))
            {
                MachineInfos.Add(info);
                foreach (var observer in observers)
                    observer.OnNext(info);
            }
            else if (MachineInfos.Count == 0)
            {
                var MachineInfosToRemove = new List<MachineInfo>();
                foreach (var machine in MachineInfos)
                {
                    if (info.MachineInfoNumber == machine.MachineInfoNumber)
                    {
                        MachineInfosToRemove.Add(machine);
                        foreach (var observer in observers)
                            observer.OnNext(info);
                    }
                }
                foreach (var machineToRemove in MachineInfosToRemove)
                    MachineInfos.Remove(machineToRemove);

                MachineInfosToRemove.Clear();
            }
        }

        public void LastMachineInfoViewed(){
            foreach (var observer in observers)
                observer.OnCompleted();

            observers.Clear();
        }
    }

    internal class Unsubscriber<MachineInfo> : IDisposable{
        private List<IObserver<MachineInfo>> _observers;
        private IObserver<MachineInfo> _observer;

        internal Unsubscriber(List<IObserver<MachineInfo>> observers, IObserver<MachineInfo> observer){
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose(){
            if (_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }

public class EmployeeMonitor : IObserver<MachineInfo>
    {
        private string role;
        private List<string> machineInfos = new List<string>();
        private IDisposable cancellation;
        //private string fmt = "{0,-20} {1,5}  {2, 3}";

        public EmployeeMonitor(string role){
            if (String.IsNullOrEmpty(role))
                throw new ArgumentNullException("The observer must be assigned a name.");
            this.role = role;
        }

        public virtual void Attach(SubjectHandler provider){
            cancellation = provider.Subscribe(this);
        }

        public virtual void Unsubscribe(){
            cancellation.Dispose();
            machineInfos.Clear();
        }

        public virtual void OnCompleted(){
            machineInfos.Clear();
        }

        // No implementation needed: Method is not called by the SubjectHandler class.
        public virtual void OnError(Exception e){
            // No implementation.
        }

        // Update information.
        public virtual void OnNext(MachineInfo info){
            bool updated = false;

            // Machines has done producing; remove from the monitor.
            if (info.MachineInfoNumber.Length == 0){
                var machinesToRemove = new List<string>();
                string machineNo = String.Format("{0,5}", info.MachineInfoNumber);

                foreach (var machineInfo in machineInfos){
                    if (machineInfo.Substring(21, 5).Equals(machineNo)){
                        machinesToRemove.Add(machineInfo);
                        updated = true;
                    }
                }
                foreach (var machineToRemove in machinesToRemove)
                    machineInfos.Remove(machineToRemove);

                machinesToRemove.Clear();
            }
            else{
                // Add Machine if it does not exist in the collection.
                string machineInfo = info.MachineInfoNumber;
                if (!machineInfos.Contains(machineInfo)){
                    machineInfos.Add(machineInfo);
                    updated = true;
                }
            }
            if (updated){
                machineInfos.Sort();
                Console.WriteLine("Machine Info {0}", this.role);
                foreach (var machineInfo in machineInfos)
                    Console.WriteLine(machineInfo);

                Console.WriteLine();
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            SubjectHandler provider = new SubjectHandler();
            EmployeeMonitor observer1 = new EmployeeMonitor("EmployeeMonitor1");
            EmployeeMonitor observer2 = new EmployeeMonitor("EmployeeMonitor2");

            provider.MachineInfoStatus("PRODUCING");
            observer1.Attach(provider);

            provider.MachineInfoStatus("IDLE");
            provider.MachineInfoStatus("IDLE");
            observer2.Attach(provider);

            provider.MachineInfoStatus("STARVED");
            provider.MachineInfoStatus("PRODUCING");

            observer2.Unsubscribe();

            provider.MachineInfoStatus("IDLE");
            provider.LastMachineInfoViewed();

            Console.ReadKey();
        }
    }
}
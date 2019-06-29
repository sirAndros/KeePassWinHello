using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.CredentialManager
{
    sealed class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
    {
        // Set the handle.
        internal CriticalCredentialHandle(IntPtr preexistingHandle)
        {
            SetHandle (preexistingHandle);
        }

        internal Credential GetCredential()
        {
            if ( !IsInvalid )
            {
                // Get the Credential from the mem location
                NativeCode.NativeCredential ncred = (NativeCode.NativeCredential) Marshal.PtrToStructure (handle,
                      typeof (NativeCode.NativeCredential));

                // Create a managed Credential type and fill it with data from the native counterpart.
                Credential cred = new Credential (ncred);
   
                return cred;
            }
            else
            {
                throw new InvalidOperationException ("Invalid CriticalHandle!");
            }
        }

        internal Credential[] EnumerateCredentials(uint size)
        {
            if (!IsInvalid)
            {
                var credentialArray = new Credential[size];

                for (int i = 0; i < size; i++)
                {
                    IntPtr ptrPlc = Marshal.ReadIntPtr(handle, i * IntPtr.Size);

                    var nc = (NativeCode.NativeCredential)Marshal.PtrToStructure(ptrPlc, typeof(NativeCode.NativeCredential));

                    credentialArray[i] = new Credential(nc);
                }

                return credentialArray;
            }
            else
            {
                throw new InvalidOperationException("Invalid CriticalHandle!");
            }
        }

        // Perform any specific actions to release the handle in the ReleaseHandle method.
        // Often, you need to use Pinvoke to make a call into the Win32 API to release the 
        // handle. In this case, however, we can use the Marshal class to release the unmanaged memory.

        override protected bool ReleaseHandle()
        {
            // If the handle was set, free it. Return success.
            if ( !IsInvalid )
            {
                // NOTE: We should also ZERO out the memory allocated to the handle, before free'ing it
                // so there are no traces of the sensitive data left in memory.
                NativeCode.CredFree (handle);
                // Mark the handle as invalid for future users.
                SetHandleAsInvalid ();
                return true;
            }
            // Return false. 
            return false;
        }
    }
}

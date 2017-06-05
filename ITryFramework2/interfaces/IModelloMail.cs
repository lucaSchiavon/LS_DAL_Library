using System;
using System.Collections.Generic;
using System.Text;

namespace it.itryframework2.interfaces
{
    public interface IModelloMail
    {
        /// <summary>
        /// Seconda operazione. A modello mail caricato riapiazzare eventuali segnaposti inseriti nel modello mail
        /// con i dati prelevati da IGenericObject.
        /// </summary>
        /// <param name="genObj">oggetto deraivante dall'interfaccia IGenericObject.</param>
        /// <returns></returns>
        string replaceBookmark(IGenericObject genObj);
        /// <summary>
        /// Prima operazione da eseguire. Carica il modello per la mail.
        /// </summary>
        void loadModello();
    }
}

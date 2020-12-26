using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Core
{

    [Serializable]
    public class AppConfig
    {
		private List<RepositoryRef> _respositoryRef;
		public List<RepositoryRef> RespositoryRefs 
		{ 
			get 
			{
				if (_respositoryRef == null)
					_respositoryRef = new List<RepositoryRef>();
				return _respositoryRef;
			}
			set { _respositoryRef = value; }
		}

        public DateTime LastDateTimeStart { get; set; }
		public int RunCounter { get; set; }
		public int AutoSaveMinutes { get; set; }

		public string CacheResources { get; set; } = @"D:\Resources\knt";
		public string CacheUrlResources { get; set; } = @"http://afx.hopto.org/kntres/NotesResources";

		#region TODO: ... other params

		//private byte _rastreoAlarmaMinutos;
		//public byte RastreoAlarmaMinutos
		//{
		//	get { return _rastreoAlarmaMinutos; }
		//	set { _rastreoAlarmaMinutos = value; }
		//}

		//private byte _autograbarMinutos;
		//public byte AutograbarMinutos
		//{
		//	get { return _autograbarMinutos; }
		//	set { _autograbarMinutos = value; }
		//}

		//private bool _activarMensajes;
		//public bool ActivarMensajes
		//{
		//	get { return _activarMensajes; }
		//	set { _activarMensajes = value; }
		//}

		//private bool _altaPrimerPlano;
		//public bool AltaPrimerPlano
		//{
		//	get { return _altaPrimerPlano; }
		//	set { _altaPrimerPlano = value; }
		//}

		//private bool _archivadorVisible = true;
		//public bool ArchivadorVisible
		//{
		//	get { return _archivadorVisible; }
		//	set { _archivadorVisible = value; }
		//}

		//private bool _soloModoTareasTmp;
		//public bool SoloModoTareasTmp
		//{
		//	get { return _soloModoTareasTmp; }
		//	set { _soloModoTareasTmp = value; }
		//}

		//private bool _archivadorMaximizado;
		//public bool ArchivadorMaximizado
		//{
		//	get { return _archivadorMaximizado; }
		//	set { _archivadorMaximizado = value; }
		//}

		//private int _archivadorTop;
		//public int ArchivadorTop
		//{
		//	get
		//	{
		//		if (_archivadorTop < 0)
		//			_archivadorTop = 0;
		//		return _archivadorTop;
		//	}
		//	set
		//	{
		//		_archivadorTop = value;
		//		if (_archivadorTop < 0)
		//			_archivadorTop = 0;
		//	}
		//}

		//private int _archivadorLeft;
		//public int ArchivadorLeft
		//{
		//	get
		//	{
		//		if (_archivadorLeft < 0)
		//			_archivadorLeft = 0;
		//		return _archivadorLeft;
		//	}
		//	set
		//	{
		//		_archivadorLeft = value;
		//		if (_archivadorLeft < 0)
		//			_archivadorLeft = 0;
		//	}
		//}

		//private int _archivadorWidth;
		//public int ArchivadorWidth
		//{
		//	get { return _archivadorWidth; }
		//	set { _archivadorWidth = value; }
		//}

		//private int _archivadorHeight;
		//public int ArchivadorHeight
		//{
		//	get { return _archivadorHeight; }
		//	set { _archivadorHeight = value; }
		//}

		//private int _contadorEjecucion;
		//public int ContadorEjecucion
		//{
		//	get { return _contadorEjecucion; }
		//	set { _contadorEjecucion = value; }
		//}

		//private DateTime _fechaHoraUltimoInicio;
		//public DateTime FechaHoraUltimoInicio
		//{
		//	get { return _fechaHoraUltimoInicio; }
		//	set { _fechaHoraUltimoInicio = value; }

		//}

		//private string _usuarioRed = "";
		//public string UsuarioRed
		//{
		//	get { return _usuarioRed; }
		//	set { _usuarioRed = value; }
		//}

		//private string _nombreComputadora = "";
		//public string NombreComputadora
		//{
		//	get { return _nombreComputadora; }
		//	set { _nombreComputadora = value; }
		//}

		//private EstiloNota _estiloNotaDefecto;
		//public EstiloNota EstiloNotaDefecto
		//{
		//	get { return _estiloNotaDefecto; }
		//	set { _estiloNotaDefecto = value; }
		//}

		//private bool _mostrarBordePostIt;
		//public bool MostrarBordePostIt
		//{
		//	get { return _mostrarBordePostIt; }
		//	set { _mostrarBordePostIt = value; }
		//}

		//private string _rutaIdsArbolCarpetas;
		//public string RutaIdsArbolCarpetas
		//{
		//	get { return _rutaIdsArbolCarpetas; }
		//	set { _rutaIdsArbolCarpetas = value; }
		//}

		//private bool _ocultarIdNota;
		//public bool OcultarIdNota
		//{
		//	get { return _ocultarIdNota; }
		//	set { _ocultarIdNota = value; }
		//}

		#endregion

	}
}

namespace Extensionista.View;

public partial class Indicadores : ContentPage
{
	public Indicadores()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void OnRegiaoClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            // Aqui voc� pode capturar o texto do bot�o clicado
            string regiaoSelecionada = button.Text;

            
            await Navigation.PushAsync(new PaginaIndicadoresRegiao(regiaoSelecionada));

            // TODO: Adicione a l�gica necess�ria para carregar dados ou navegar para outra p�gina
        }
    }

}
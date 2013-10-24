using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Terminals.Data;

namespace Terminals
{
    internal partial class OrganizeGroupsForm : Form
    {
        internal OrganizeGroupsForm()
        {
            InitializeComponent();

            this.gridGroups.AutoGenerateColumns = false;
            this.gridGroupFavorites.AutoGenerateColumns = false;
            LoadGroups();
        }

        private void LoadGroups()
        {
            var groups = Persistence.Instance.Groups.ToList();
            this.gridGroups.DataSource = groups;
        }

        private IGroup GetSelectedGroup()
        {
            if (gridGroups.SelectedRows.Count > 0)
                return gridGroups.SelectedRows[0].DataBoundItem as IGroup;
            return null;
        }

        private void LoadSelectedGroupFavorites()
        {
            IGroup selectedGroup = GetSelectedGroup();
            if (selectedGroup != null)
            {
                this.gridGroupFavorites.DataSource = selectedGroup.Favorites;
            }
        }

        private void GridGroups_SelectedRowChanged(object sender, EventArgs e)
        {
            LoadSelectedGroupFavorites();
        }

        private void TsbAddGroup_Click(object sender, EventArgs e)
        {
            string newGroupName = NewGroupForm.AskFroGroupName();
            if (string.IsNullOrEmpty(newGroupName))
                return;
            
            FavoritesFactory.GetOrAddNewGroup(newGroupName);   
            this.LoadGroups();
        }

        private void TsbDeleteGroup_Click(object sender, EventArgs e)
        {
            IGroup group = GetSelectedGroup();
            if (group != null && OrganizeFavoritesForm.AskIfRealyDelete("group"))
            {
                Persistence.Instance.Groups.Delete(group);
                LoadGroups();
            }
        }

        private void TsbAddConnection_Click(object sender, EventArgs e)
        {
            IGroup group = GetSelectedGroup();
            if (group != null)
                this.AddFavoritesToGroup(group);
        }

        private void AddFavoritesToGroup(IGroup group)
        {
            using (var frmAddConnection = new AddConnectionForm(Persistence.Instance))
            {
                if (frmAddConnection.ShowDialog() == DialogResult.OK)
                {
                    group.AddFavorites(frmAddConnection.SelectedFavorites);
                    this.LoadSelectedGroupFavorites();
                    Persistence.Instance.SaveAndFinishDelayedUpdate();
                }
            }
        }

        private void TsbDeleteConnection_Click(object sender, EventArgs e)
        {
            IGroup group = GetSelectedGroup();
            if (group != null)
            {
                if (this.gridGroupFavorites.SelectedRows.Count > 0)
                {
                    List<IFavorite> selectedFavorites = GetSelectedFavorites();
                    group.RemoveFavorites(selectedFavorites);
                    LoadSelectedGroupFavorites();
                    Persistence.Instance.SaveAndFinishDelayedUpdate();
                }
            }
        }

        private List<IFavorite> GetSelectedFavorites()
        {
            var selectedRows = this.gridGroupFavorites.SelectedRows.Cast<DataGridViewRow>();
            return selectedRows.Select(row => row.DataBoundItem as IFavorite)
                .ToList();
        }
    }
}
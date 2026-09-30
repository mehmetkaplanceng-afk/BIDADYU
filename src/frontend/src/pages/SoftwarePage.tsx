import React, { useEffect, useState } from 'react';
import { Table, Card, Tag, Button, Modal, Form, Input, Popconfirm, message } from 'antd';
import { PlusOutlined, DeleteOutlined, PlusCircleOutlined } from '@ant-design/icons';
import api, { getSoftware } from '../services/api';

export const SoftwarePage: React.FC = () => {
  const [softwareList, setSoftwareList] = useState<any[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [isSoftwareModalOpen, setIsSoftwareModalOpen] = useState<boolean>(false);
  const [isVersionModalOpen, setIsVersionModalOpen] = useState<boolean>(false);
  const [selectedSoftwareId, setSelectedSoftwareId] = useState<string | null>(null);

  const [softwareForm] = Form.useForm();
  const [versionForm] = Form.useForm();

  const fetchSoftware = async () => {
    setLoading(true);
    try {
      const res = await getSoftware();
      setSoftwareList(res.data);
    } catch (err) {
      message.error('Yazılımlar yüklenemedi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSoftware();
  }, []);

  const handleCreateSoftware = async (values: any) => {
    try {
      await api.post('/Software', values);
      message.success('Yeni yazılım kataloğa eklendi!');
      setIsSoftwareModalOpen(false);
      softwareForm.resetFields();
      fetchSoftware();
    } catch (err) {
      message.error('Yazılım eklenemedi.');
    }
  };

  const handleAddVersion = async (values: any) => {
    if (!selectedSoftwareId) return;
    try {
      await api.post(`/Software/${selectedSoftwareId}/versions`, values);
      message.success('Sürüm eklendi!');
      setIsVersionModalOpen(false);
      versionForm.resetFields();
      fetchSoftware();
    } catch (err) {
      message.error('Sürüm eklenemedi.');
    }
  };

  const handleDeleteSoftware = async (id: string) => {
    try {
      await api.delete(`/Software/${id}`);
      message.success('Yazılım silindi.');
      fetchSoftware();
    } catch (err) {
      message.error('Silme başarısız.');
    }
  };

  const columns = [
    { title: 'Yazılım Adı', dataIndex: 'name', key: 'name', sorter: (a: any, b: any) => a.name.localeCompare(b.name) },
    { title: 'Yayıncı / Üretici', dataIndex: 'publisher', key: 'publisher' },
    { title: 'Açıklama', dataIndex: 'description', key: 'description' },
    {
      title: 'Sürümler',
      dataIndex: 'versions',
      key: 'versions',
      render: (versions: any[]) =>
        versions && versions.length > 0
          ? versions.map((v) => <Tag color={v.isCurrent ? 'green' : 'default'} key={v.id}>{v.version}</Tag>)
          : <Tag color="orange">Henüz Sürüm Yok</Tag>,
    },
    {
      title: 'İşlemler',
      key: 'action',
      render: (_: any, record: any) => (
        <div style={{ display: 'flex', gap: 8 }}>
          <Button
            size="small"
            icon={<PlusCircleOutlined />}
            onClick={() => {
              setSelectedSoftwareId(record.id);
              setIsVersionModalOpen(true);
            }}
          >
            Sürüm Ekle
          </Button>
          <Popconfirm title="Yazılımı silmek istediğinize emin misiniz?" onConfirm={() => handleDeleteSoftware(record.id)}>
            <Button danger size="small" icon={<DeleteOutlined />}>
              Sil
            </Button>
          </Popconfirm>
        </div>
      ),
    },
  ];

  return (
    <Card title="Yazılım Kataloğu Yönetimi" extra={<Button type="primary" icon={<PlusOutlined />} onClick={() => setIsSoftwareModalOpen(true)}>Yeni Yazılım Ekle</Button>}>
      <Table dataSource={softwareList} columns={columns} rowKey="id" loading={loading} />

      {/* Yeni Yazılım Ekle Modal */}
      <Modal title="Kataloğa Yeni Yazılım Ekle" open={isSoftwareModalOpen} onCancel={() => setIsSoftwareModalOpen(false)} onOk={() => softwareForm.submit()}>
        <Form form={softwareForm} layout="vertical" onFinish={handleCreateSoftware}>
          <Form.Item name="name" label="Yazılım Adı" rules={[{ required: true, message: 'Yazılım adı giriniz!' }]}>
            <Input placeholder="Örn: Google Chrome, Adobe Reader" />
          </Form.Item>
          <Form.Item name="publisher" label="Yayıncı / Üretici">
            <Input placeholder="Örn: Google LLC, Adobe Systems" />
          </Form.Item>
          <Form.Item name="description" label="Açıklama">
            <Input.TextArea placeholder="Yazılımla ilgili açıklama giriniz." />
          </Form.Item>
        </Form>
      </Modal>

      {/* Sürüm Ekle Modal */}
      <Modal title="Yazılıma Yeni Sürüm Ekle" open={isVersionModalOpen} onCancel={() => setIsVersionModalOpen(false)} onOk={() => versionForm.submit()}>
        <Form form={versionForm} layout="vertical" onFinish={handleAddVersion}>
          <Form.Item name="version" label="Sürüm Numarası" rules={[{ required: true, message: 'Sürüm giriniz!' }]}>
            <Input placeholder="Örn: 140.0.1 veya 2024.1" />
          </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
};
